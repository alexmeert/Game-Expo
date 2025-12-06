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

		_gun = GetNode<Gun>("Gun");
		if (_gun != null)
		{
			_gun.Owner = this;
			UpdateAmmoUI();
		}

		if (ReloadBar != null) ReloadBar.Visible = false;
		if (AmmoBar != null) AmmoBar.Visible = true;
		if (OverclockAura != null) OverclockAura.Visible = false;
		if (FirewallAura != null) FirewallAura.Visible = false;

		if (GlobalInventory.Instance != null)
		{
			foreach (var upgrade in GlobalInventory.Instance.GetUpgrades())
				ApplyUpgrade(upgrade);
		}

		UpdateStatsLabels();
	}

	protected override void InitializeEntity()
	{
		base.InitializeEntity();

		SetStats(Hp, Dmg, AtkSpd, Def, Spd);

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
			InventoryUI.Instance?.Toggle();
	}

	protected override void HandleMovement(double delta)
	{
		Vector2 input = GetInput();

		if (input.Length() > 0)
		{
			if (!WalkSound.Playing) WalkSound.Play();
			Velocity = Velocity.Lerp(input * Spd, (float)delta * ACCEL);
		}
		else
		{
			Velocity = Velocity.Lerp(Vector2.Zero, (float)delta * FRICTION);
			WalkSound.Stop();
		}

		Position += Velocity * (float)delta;

		var animSprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");
		if (animSprite != null)
		{
			if (input.Length() == 0) animSprite.Play("Idle");
			else if (Mathf.Abs(input.X) > Mathf.Abs(input.Y))
				animSprite.Play(input.X > 0 ? "MoveRight" : "MoveLeft");
			else
				animSprite.Play(input.Y > 0 ? "MoveDown" : "MoveUp");
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
		if (_gun == null) return;

		if (AmmoLabel != null)
			AmmoLabel.Text = _gun.IsReloading ? "reloading" : $"{_gun.CurrentAmmo} / {_gun.MaxAmmoValue}";

		if (AmmoBar != null)
		{
			AmmoBar.Visible = !_gun.IsReloading;
			if (!_gun.IsReloading)
				AmmoBar.Value = (float)_gun.CurrentAmmo / _gun.MaxAmmoValue;
		}

		if (ReloadBar != null)
		{
			ReloadBar.Visible = _gun.IsReloading;
			ReloadBar.Value = _gun.IsReloading ? _gun.ReloadProgress : 0f;
		}
	}

	protected override void Die()
	{
		DeathSound?.Play();
		GD.Print("Player died!");

		SetProcess(false);
		SetPhysicsProcess(false);

		var animSprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");

		async void StartDeathSequence()
		{
			var animSprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");

			if (animSprite != null)
			{
				var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();

				Action handler = null;
				handler = () =>
				{
					if (animSprite.Animation == "Death")
					{
						tcs.TrySetResult(true);
						animSprite.AnimationFinished -= handler;
					}
				};

				animSprite.AnimationFinished += handler;
				animSprite.Play("Death");

				await tcs.Task;
			}

			if (ScreenFader.Instance != null)
				await ScreenFader.Instance.FadeOut();

			GetTree().ChangeSceneToFile("res://Scenes/Menus/DeathMenu.tscn");
			QueueFree();
		}



		StartDeathSequence();
	}

	public void ApplyUpgrade(Upgrade upgrade)
	{
		if (upgrade == null) return;
		upgrade.Apply(this);
		_activeUpgrades.Add(upgrade);
		UpdateStatsLabels();
	}

	public void ApplyPerk(Perk perk)
	{
		if (perk == null) return;

		if (_activePerks.Contains(perk))
		{
			perk.Apply(this);
			UpdateStatsLabels();
			return;
		}

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

			if (perk.IsActive)
			{
				string perkName = perk.ItemName?.ToLower() ?? "";
				if (perkName.Contains("overclock")) hasOverclock = true;
				if (perkName.Contains("firewall")) hasFirewall = true;
			}

			if (!perk.IsActive)
			{
				perk.Remove(this);
				_activePerks.RemoveAt(i);
				UpdateStatsLabels();
			}
		}

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
			if (!aura.Visible)
			{
				aura.Visible = true;
				aura.Play("Active");
			}
		}
		else
		{
			if (aura.Visible)
			{
				aura.Play("Expire");

				Action handler = null;
				handler = () =>
				{
					if (aura.Animation == "Expire")
					{
						aura.Visible = false;
						aura.AnimationFinished -= handler;
					}
				};

				aura.AnimationFinished += handler;
			}
		}
	}



	public List<Upgrade> GetActiveUpgrades() => new List<Upgrade>(_activeUpgrades);
	public List<Perk> GetActivePerks() => new List<Perk>(_activePerks);

	public float GetBaseMaxHP() => _baseMaxHP;
	public float GetBaseDMG() => _baseDMG;
	public float GetBaseATKSPD() => _baseATKSPD;
	public float GetBaseDEF() => _baseDEF;
	public float GetBaseSPD() => _baseSPD;

	private void UpdateStatsLabels()
	{
		if (DamageLabel != null) DamageLabel.Text = $"Damage: {Mathf.RoundToInt(DMG)}";
		if (DefenseLabel != null) DefenseLabel.Text = $"Defense: {(DEF * 100f):F1}%";
		if (FireRateLabel != null) FireRateLabel.Text = $"Fire Rate: {ATKSPD:F2}/s";
		if (CooldownLabel != null && _gun != null) CooldownLabel.Text = $"Cooldown: {_gun.GetFireCooldown():F3}s";
		if (MagSizeLabel != null && _gun != null) MagSizeLabel.Text = $"Mag Size: {_gun.MaxAmmoValue}";
		if (HealthLabel != null) HealthLabel.Text = $"Health: {Mathf.CeilToInt(HP)} / {Mathf.CeilToInt(MaxHP)}";
	}
}
