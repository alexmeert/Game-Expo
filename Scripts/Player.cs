using Godot;
using System;
using System.Collections.Generic;

public partial class Player : BasicEntity
{
	private const float ACCEL = 15.0f;
	private const float FRICTION = 12.0f;

	[Export] private int Hp = 100;
	[Export] private int Dmg = 10;
	[Export] private float AtkSpd = 1.0f;
	[Export] private float Def = 0f; // Percentage (0.0 = 0%, 1.0 = 100% damage reduction)
	[Export] private float Spd = 100;
	[Export] private AudioStreamPlayer2D HitSound;
	[Export] private AudioStreamPlayer2D DeathSound;
	[Export] private AudioStreamPlayer2D WalkSound;
	[Export] private Label HPValueLabel;
	[Export] private Label AmmoLabel;
	[Export] private ProgressBar AmmoBar;
	[Export] private ProgressBar ReloadBar;

	private List<Upgrade> _activeUpgrades = new List<Upgrade>();
	private List<Perk> _activePerks = new List<Perk>();

	// Base stats (before upgrades) - used for percentage calculations
	private float _baseMaxHP;
	private float _baseDMG;
	private float _baseATKSPD;
	private float _baseDEF;
	private float _baseSPD;

	private Gun _gun;

	public override void _Ready()
	{
		base._Ready();

		// Assign gun owner first
		_gun = GetNode<Gun>("Gun");
		if (_gun != null)
		{
			_gun.Owner = this;
			UpdateAmmoUI();
		}

		// Hide reload bar initially, show ammo bar
		if (ReloadBar != null)
		{
			ReloadBar.Visible = false;
		}
		if (AmmoBar != null)
		{
			AmmoBar.Visible = true;
		}

		// Apply all previously collected upgrades from GlobalInventory
		foreach (var upgrade in GlobalInventory.Instance.GetUpgrades())
		{
			ApplyUpgrade(upgrade);
		}
	}



	protected override void InitializeEntity()
	{
		base.InitializeEntity();

		SetStats(
			hp: Hp,
			dmg: Dmg,
			atkspd: AtkSpd,
			def: Def,
			spd: Spd
		);

		// Store base stats for percentage calculations
		_baseMaxHP = MaxHP;
		_baseDMG = DMG;
		_baseATKSPD = ATKSPD;
		_baseDEF = DEF;
		_baseSPD = SPD;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		UpdatePerks((float)delta);
		UpdateAmmoUI();
	}
	
	public override void _Input(InputEvent @event)
{
	if (@event.IsActionPressed("inventory_toggle"))
	{
		InventoryUI.Instance?.Toggle();
	}
}


	protected override void HandleMovement(double delta)
	{
		Vector2 input = GetInput();

		if (input.Length() > 0)
		{
			if (!WalkSound.Playing) // prevents restarting every frame
				WalkSound.Play();
			Velocity = Velocity.Lerp(input * Spd, (float)delta * ACCEL);
		}
		else
		{
			Velocity = Velocity.Lerp(Vector2.Zero, (float)delta * FRICTION);
			WalkSound.Stop();
		}

		// Move the player
		Position += Velocity * (float)delta;

		// Handle animation
		var animSprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");
		if (animSprite != null)
		{
			if (input.Length() == 0)
			{
				animSprite.Play("Idle");
			}
			else
			{
				if (Mathf.Abs(input.X) > Mathf.Abs(input.Y))
				{
					animSprite.Play(input.X > 0 ? "MoveRight" : "MoveLeft");
				}
				else
				{
					animSprite.Play(input.Y > 0 ? "MoveDown" : "MoveUp");
				}
			}
		}
	}



	private Vector2 GetInput()
	{
		float inputX = Input.GetActionStrength("D") - Input.GetActionStrength("A");
		float inputY = Input.GetActionStrength("S") - Input.GetActionStrength("W");

		Vector2 vec = new Vector2(inputX, inputY);

		return vec.Length() > 0 ? vec.Normalized() : Vector2.Zero;
	}

	protected override void OnTakeDamage(float damage)
	{
		base.OnTakeDamage(damage);
		HitSound?.Play();
		UpdateHPLabel();
	}

	protected override void OnHPChanged()
	{
		base.OnHPChanged();
		UpdateHPLabel();
	}

	protected override void OnMaxHPChanged()
	{
		base.OnMaxHPChanged();
		UpdateHPLabel();
	}

	private void UpdateHPLabel()
	{
		if (HPValueLabel != null)
			HPValueLabel.Text = $"{Mathf.CeilToInt(HP)} / {Mathf.CeilToInt(MaxHP)}";
	}

	private void UpdateAmmoUI()
	{
		if (_gun == null)
			return;

		// Update ammo label
		if (AmmoLabel != null)
		{
			if (_gun.IsReloading)
			{
				AmmoLabel.Text = "reloading";
			}
			else
			{
				AmmoLabel.Text = $"{_gun.CurrentAmmo} / {_gun.MaxAmmoValue}";
			}
		}

		// Update ammo bar (depletes as you shoot)
		if (AmmoBar != null)
		{
			if (_gun.IsReloading)
			{
				// Hide ammo bar when reloading
				AmmoBar.Visible = false;
			}
			else
			{
				// Show ammo bar and update value based on remaining ammo
				AmmoBar.Visible = true;
				float ammoPercent = (float)_gun.CurrentAmmo / (float)_gun.MaxAmmoValue;
				AmmoBar.Value = ammoPercent; // ProgressBar uses 0-1
			}
		}

		// Update reload bar (fills when reloading)
		if (ReloadBar != null)
		{
			if (_gun.IsReloading)
			{
				ReloadBar.Visible = true;
				// ReloadProgress goes from 0 to 1 as reload progresses
				ReloadBar.Value = _gun.ReloadProgress; // ProgressBar uses 0-1
			}
			else
			{
				ReloadBar.Visible = false;
				ReloadBar.Value = 0f; // Reset when not reloading
			}
		}
	}

	protected override void Die()
	{
		base.Die();
		
		if (DeathSound != null)
		{
			DeathSound.Play();
		}
		
		GD.Print("Player died!");
		
		GetTree().ChangeSceneToFile("res://Scenes/Menus/DeathMenu.tscn");
	}

	/// <summary>
	/// Applies a permanent upgrade to the player
	/// </summary>
	public void ApplyUpgrade(Upgrade upgrade)
	{
		if (upgrade == null)
			return;

		upgrade.Apply(this);      // change stats
		_activeUpgrades.Add(upgrade); 

		
	}



	/// <summary>
	/// Applies a temporary perk to the player
	/// </summary>
	public void ApplyPerk(Perk perk)
	{
		if (perk == null)
			return;

		// If perk is not in the list, add it
		if (!_activePerks.Contains(perk))
		{
			_activePerks.Add(perk);
		}

		// Apply or refresh the perk
		perk.Apply(this);
	}

	/// <summary>
	/// Updates all active perks and removes expired ones
	/// </summary>
	private void UpdatePerks(float delta)
	{
		for (int i = _activePerks.Count - 1; i >= 0; i--)
		{
			Perk perk = _activePerks[i];
			perk.Update(delta);

			if (!perk.IsActive)
			{
				perk.Remove(this);
				_activePerks.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// Gets all active upgrades
	/// </summary>
	public List<Upgrade> GetActiveUpgrades()
	{
		return new List<Upgrade>(_activeUpgrades);
	}

	/// <summary>
	/// Gets all active perks
	/// </summary>
	public List<Perk> GetActivePerks()
	{
		return new List<Perk>(_activePerks);
	}

	// Base stat getters for upgrade percentage calculations
	public float GetBaseMaxHP() => _baseMaxHP;
	public float GetBaseDMG() => _baseDMG;
	public float GetBaseATKSPD() => _baseATKSPD;
	public float GetBaseDEF() => _baseDEF;
	public float GetBaseSPD() => _baseSPD;
}
