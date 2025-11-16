using Godot;

public partial class Gun : Node2D
{
    [Export] public PlayerProjectile PlayerProjectileManager;
    [Export] public Marker2D Muzzle;
    [Export] public AudioStreamPlayer2D ShotSound;

    [Export] public float FireCooldown = 0.50f;
    private float fireTimer = 0f;

    public BasicEntity Owner { get; set; }

    public override void _Process(double delta)
    {
        LookAt(GetGlobalMousePosition());
        RotationDegrees = Mathf.Wrap(RotationDegrees, 0f, 360f);
        Scale = new Vector2(Scale.X, (RotationDegrees > 90 && RotationDegrees < 270) ? -1 : 1);

        // Count down cooldown timer
        if (fireTimer > 0)
            fireTimer -= (float)delta;

        // Only fire if cooldown finished
        if (Input.IsActionPressed("Shoot") && fireTimer <= 0f)
        {
            FireProjectile();
        }
    }

    private void FireProjectile()
    {
        if (PlayerProjectileManager == null || Muzzle == null || Owner == null)
            return;

        fireTimer = FireCooldown;

        PlayerProjectileManager.Owner = Owner;

        PlayerProjectileManager.SpawnProjectile(
            Muzzle.GlobalPosition,
            GlobalRotation
        );

        ShotSound?.Play();
    }
}
