using Godot;

public partial class Gun : Node2D
{
    [Export] public PlayerProjectile PlayerProjectileManager;
    [Export] public Marker2D Muzzle;
    [Export] public AudioStreamPlayer2D ShotSound;

    [Export] public float FireCooldown = 0.20f;
    private float fireTimer = 0f;

    public BasicEntity Owner { get; set; }

    public override void _Process(double delta)
    {
        if (fireTimer > 0)
            fireTimer -= (float)delta;

        if (Input.IsActionPressed("Shoot") && fireTimer <= 0f)
        {
            FireProjectile();
        }
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
