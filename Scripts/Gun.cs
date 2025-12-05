using Godot;

public partial class Gun : Node2D
{
	[Export] public PlayerProjectile PlayerProjectileManager;
	[Export] public Marker2D Muzzle;
	[Export] public AudioStreamPlayer2D ShotSound;

	[Export] public float FireCooldown = 0.20f;
	[Export] public int MaxAmmo = 10;
	[Export] public float ReloadTime = 2.0f;

	private float fireTimer = 0f;
	private int currentAmmo;
	private float reloadTimer = 0f;
	private bool isReloading = false;

	public BasicEntity Owner { get; set; }

	public int CurrentAmmo => currentAmmo;
	public int MaxAmmoValue => MaxAmmo;
	public bool IsReloading => isReloading;
	public float ReloadProgress => isReloading ? (ReloadTime - reloadTimer) / ReloadTime : 0f;

	public override void _Ready()
	{
		currentAmmo = MaxAmmo;
	}

	public override void _Process(double delta)
	{
		float deltaFloat = (float)delta;

		// Update fire cooldown
		if (fireTimer > 0)
			fireTimer -= deltaFloat;

		// Handle reloading
		if (isReloading)
		{
			reloadTimer -= deltaFloat;
			if (reloadTimer <= 0f)
			{
				FinishReload();
			}
		}
		// Auto-reload when out of ammo
		else if (currentAmmo <= 0)
		{
			StartReload();
		}
		// Handle shooting
		else if (Input.IsActionPressed("Shoot") && fireTimer <= 0f)
		{
			FireProjectile();
		}
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
		reloadTimer = 0f;
	}

	private void FireProjectile()
	{
		if (PlayerProjectileManager == null)
		{
			return;
		}

		if (Owner == null)
		{
			return;
		}

		// Check if we have ammo
		if (currentAmmo <= 0)
		{
			return;
		}

		// Consume ammo
		currentAmmo--;
		fireTimer = FireCooldown;

		// Calculate direction to mouse
		Vector2 mousePos = GetGlobalMousePosition();
		Vector2 spawnPosition;
		
		// Use Muzzle position if available, otherwise use owner's position
		if (Muzzle != null)
		{
			spawnPosition = Muzzle.GlobalPosition;
		}
		else
		{
			spawnPosition = Owner.GlobalPosition;
		}

		// Calculate rotation from spawn position to mouse
		Vector2 direction = (mousePos - spawnPosition);
		
		// Check if direction is valid (not zero)
		if (direction.LengthSquared() < 0.01f)
		{
			// Default direction if mouse is too close
			direction = Vector2.Right;
		}
		else
		{
			direction = direction.Normalized();
		}
		
		float rotation = direction.Angle();

		PlayerProjectileManager.Owner = Owner;

		PlayerProjectileManager.SpawnProjectile(
			spawnPosition,
			rotation
		);

		if (ShotSound != null)
		{
			ShotSound.Play();
		}
	}
}
