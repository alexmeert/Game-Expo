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
	[Export] private float Def = 0f;
	[Export] private float Spd = 100;
	[Export] private AudioStreamPlayer2D HitSound;
	[Export] private AudioStreamPlayer2D DeathSound;
	[Export] private AudioStreamPlayer2D WalkSound;
	[Export] private Label HPValueLabel;
	[Export] private Label AmmoLabel;
	[Export] private ProgressBar AmmoBar;
	[Export] private ProgressBar ReloadBar;
	[Export] private AnimatedSprite2D OverclockAura;
	[Export] private AnimatedSprite2D FirewallAura;
	
	// Stat display labels
	[Export] private Label DamageLabel;
	[Export] private Label DefenseLabel;
	[Export] private Label FireRateLabel;
	[Export] private Label CooldownLabel;
	[Export] private Label MagSizeLabel;
	[Export] private Label HealthLabel;

	private List<Upgrade> _activeUpgrades = new List<Upgrade>();
	private List<Perk> _activePerks = new List<Perk>();

	private float _baseMaxHP;
	private float _baseDMG;
	private float _baseATKSPD;
	private float _baseDEF;
	private float _baseSPD;

	private Gun _gun;

	public Gun Gun => _gun;
	public override void _Ready()
	{
		base._Ready();

		// InitializeEntity is called in base._Ready(), so base stats are set
		// Now apply upgrades after base stats are established
		
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

		// Hide auras initially
		if (OverclockAura != null)
		{
			OverclockAura.Visible = false;
		}
		if (FirewallAura != null)
		{
			FirewallAura.Visible = false;
		}

		// Apply all previously collected upgrades from GlobalInventory
		// This happens after InitializeEntity, so base stats are already set
		if (GlobalInventory.Instance != null)
		{
			foreach (var upgrade in GlobalInventory.Instance.GetUpgrades())
			{
				ApplyUpgrade(upgrade);
			}
		}
		
		// Initialize stat labels
		UpdateStatsLabels();
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
		UpdateStatsLabels();
	}

	protected override void OnMaxHPChanged()
	{
		base.OnMaxHPChanged();
		UpdateHPLabel();
		UpdateStatsLabels();
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

		// Update ammo bar 
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
				AmmoBar.Value = ammoPercent; 
			}
		}

		// Update reload bar
		if (ReloadBar != null)
		{
			if (_gun.IsReloading)
			{
				ReloadBar.Visible = true;
				
				ReloadBar.Value = _gun.ReloadProgress; 
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
		// Call OnDeath() to emit signals, but don't QueueFree yet
		OnDeath();
		
		var animSprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");
		if (animSprite != null)
		{
			// Play death animation
			animSprite.Play("Death");
			
			// Wait for animation to finish before changing scene
			if (!animSprite.IsConnected(AnimatedSprite2D.SignalName.AnimationFinished, new Callable(this, nameof(OnDeathAnimationFinished))))
			{
				animSprite.AnimationFinished += OnDeathAnimationFinished;
			}
		}
		else
		{
			// No sprite, go directly to death menu
			OnDeathAnimationFinished();
		}
		
		if (DeathSound != null)
		{
			DeathSound.Play();
		}
	}
	
	private void OnDeathAnimationFinished()
	{
		var animSprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");
		// Only proceed if the death animation finished (or if there's no sprite)
		if (animSprite == null || animSprite.Animation == "Death")
		{
			// Animation finished, now change scene
			GetTree().ChangeSceneToFile("res://Scenes/Menus/DeathMenu.tscn");
			// QueueFree will happen automatically when scene changes
			QueueFree();
		}
	}

	public void ApplyUpgrade(Upgrade upgrade)
	{
		if (upgrade == null)
			return;

		upgrade.Apply(this);
		_activeUpgrades.Add(upgrade);
		UpdateStatsLabels();
	}

	public void ApplyPerk(Perk perk)
	{
		if (perk == null)
			return;

		// Check if we already have this exact perk instance
		if (_activePerks.Contains(perk))
		{
			// If it's already in the list, just refresh it
			perk.Apply(this);
			UpdateStatsLabels();
			return;
		}

		// Add to list and apply
		_activePerks.Add(perk);
		perk.Apply(this);
		UpdateStatsLabels();
	}

	private void UpdatePerks(float delta)
	{
		bool hasOverclock = false;
		bool hasFirewall = false;

		for (int i = _activePerks.Count - 1; i >= 0; i--)
		{
			Perk perk = _activePerks[i];
			perk.Update(delta);

			// Check if perk is active and matches aura names
			if (perk.IsActive)
			{
				string perkName = perk.ItemName?.ToLower() ?? "";
				if (perkName.Contains("overclock"))
				{
					hasOverclock = true;
					GD.Print($"Found active Overclock perk: {perk.ItemName}");
				}
				if (perkName.Contains("firewall"))
				{
					hasFirewall = true;
					GD.Print($"Found active Firewall perk: {perk.ItemName}");
				}
			}

			if (!perk.IsActive)
			{
				perk.Remove(this);
				_activePerks.RemoveAt(i);
				UpdateStatsLabels(); // Update stats when perk is removed
			}
		}

		// Update aura visibility and animation
		UpdateAuras(hasOverclock, hasFirewall);
	}

	private void UpdateAuras(bool hasOverclock, bool hasFirewall)
	{
		HandleAura(OverclockAura, hasOverclock);
		HandleAura(FirewallAura, hasFirewall);
	}

	private void HandleAura(AnimatedSprite2D aura, bool isActive)
	{
		if (aura == null) return;

		if (isActive)
		{
			// Activate if not already visible
			if (!aura.Visible)
			{
				aura.Visible = true;
				aura.Play("Active");
			}
		}
		else
		{
			// Only trigger expire if it was active before
			if (aura.Visible)
			{
				aura.Play("Expire");

				// Wait for expire animation to finish
				aura.AnimationFinished += () =>
				{
					if (aura.Animation == "Expire")
					{
						aura.Visible = false;
					}
				};

			}
		}
	}


	public List<Upgrade> GetActiveUpgrades()
	{
		return new List<Upgrade>(_activeUpgrades);
	}

	public List<Perk> GetActivePerks()
	{
		return new List<Perk>(_activePerks);
	}

	public float GetBaseMaxHP() => _baseMaxHP;
	public float GetBaseDMG() => _baseDMG;
	public float GetBaseATKSPD() => _baseATKSPD;
	public float GetBaseDEF() => _baseDEF;
	public float GetBaseSPD() => _baseSPD;
	
	private void UpdateStatsLabels()
	{
		// Update Damage label
		if (DamageLabel != null)
		{
			DamageLabel.Text = $"Damage: {Mathf.RoundToInt(DMG)}";
		}
		
		// Update Defense label (show as percentage)
		if (DefenseLabel != null)
		{
			DefenseLabel.Text = $"Defense: {(DEF * 100f):F1}%";
		}
		
		// Update Fire Rate label (show as attacks per second)
		if (FireRateLabel != null)
		{
			FireRateLabel.Text = $"Fire Rate: {ATKSPD:F2}/s";
		}
		
		// Update Cooldown label (show actual fire cooldown in seconds)
		if (CooldownLabel != null && _gun != null)
		{
			float cooldown = _gun.GetFireCooldown();
			CooldownLabel.Text = $"Cooldown: {cooldown:F3}s";
		}
		
		// Update Magazine Size label
		if (MagSizeLabel != null && _gun != null)
		{
			MagSizeLabel.Text = $"Mag Size: {_gun.MaxAmmoValue}";
		}
		
		// Update Health label (show current/max)
		if (HealthLabel != null)
		{
			HealthLabel.Text = $"Health: {Mathf.CeilToInt(HP)} / {Mathf.CeilToInt(MaxHP)}";
		}
	}
}
