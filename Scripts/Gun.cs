using Godot;

public partial class Gun : Node2D
{
	[Export] public PlayerProjectile PlayerProjectileManager;
	[Export] public Marker2D Muzzle;
	[Export] public AudioStreamPlayer2D ShotSound;

	[Export] public float BaseFireCooldown = 0.20f;
	[Export] public int MaxAmmo = 10;
	[Export] public float ReloadTime = 2.0f;

	private float fireTimer = 0f;
	private int currentAmmo;
	private float reloadTimer = 0f;
	private bool isReloading = false;

	public BasicEntity Owner { get; set; }
	
	public float GetFireCooldown()
	{
		if (Owner == null)
			return BaseFireCooldown;
		
		// Check if Overclock perk is active (0 cooldown)
		if (Owner is Player player)
		{
			foreach (var perk in player.GetActivePerks())
			{
				if (perk.IsActive)
				{
					string perkName = perk.ItemName?.ToLower() ?? "";
					if (perkName.Contains("overclock"))
					{
						return 0f; // Overclock gives 0 cooldown
					}
				}
			}
		}
		
		if (Owner.ATKSPD <= 0f)
			return BaseFireCooldown;
		
		return BaseFireCooldown / Owner.ATKSPD;
	}

	public int CurrentAmmo => currentAmmo;
	public bool IsReloading => isReloading;

	public int MaxAmmoValue => MaxAmmo;
	public float ReloadProgress => isReloading ? 1f - (reloadTimer / ReloadTime) : 0f;

	public override void _Ready()
	{
		currentAmmo = MaxAmmo;
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;

		if (fireTimer > 0)
			fireTimer -= dt;

		if (isReloading)
		{
			reloadTimer -= dt;
			if (reloadTimer <= 0)
				FinishReload();
		}
		else if (currentAmmo <= 0)
		{
			StartReload();
		}
		else if (Input.IsActionPressed("Shoot") && fireTimer <= 0f)
		{
			FireProjectile();
		}
	}

	public void AddMagazineSize(int amount)
	{
		if (amount == 0) return;

		if (amount > 0)
		{
			MaxAmmo += amount;
			currentAmmo = MaxAmmo;
		}
		else
		{
			MaxAmmo = Mathf.Max(0, MaxAmmo + amount);
			currentAmmo = Mathf.Min(currentAmmo, MaxAmmo);
		}
	}

	public void AddAmmo(int amount)
	{
		currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, MaxAmmo);
	}

	private void StartReload()
	{
		if (isReloading || currentAmmo >= MaxAmmo)
			return;

		isReloading = true;
		reloadTimer = ReloadTime;
	}

	private void FinishReload()
	{
		isReloading = false;
		currentAmmo = MaxAmmo;
	}

	private void FireProjectile()
	{
		if (PlayerProjectileManager == null || Owner == null)
			return;

		if (currentAmmo <= 0)
			return;

		currentAmmo--;
		fireTimer = GetFireCooldown();

		Vector2 spawnPos = Muzzle != null ? Muzzle.GlobalPosition : Owner.GlobalPosition;

		Vector2 mousePos = GetGlobalMousePosition();
		Vector2 dir = mousePos - spawnPos;

		if (dir.LengthSquared() < 0.01f)
			dir = Vector2.Right;
		else
			dir = dir.Normalized();

		float rot = dir.Angle();

		PlayerProjectileManager.Owner = Owner;

		PlayerProjectileManager.SpawnProjectile(
			spawnPos,
			rot
		);

		ShotSound?.Play();
	}
}
