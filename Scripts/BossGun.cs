using Godot;

public partial class BossGun : Node2D
{
    [Export] public Marker2D Muzzle;
    [Export] public BossProjectileManager BossProjectileManager;
    [Export] public AudioStreamPlayer2D ShotSound;

    private float attackTimer = 0f;
    private int phase = 1;

    public void SetPhase(int newPhase)
    {
        phase = newPhase;
        attackTimer = 1f;
    }

    public override void _Process(double delta)
    {
        if (attackTimer > 0)
            attackTimer -= (float)delta;
    }

    public void TryFire()
    {
        if (BossProjectileManager == null || BossProjectileManager.Owner == null)
            return;

        if (attackTimer <= 0f)
        {
            attackTimer = phase switch
            {
                1 => 1.2f,
                2 => 0.7f,
                3 => 0.30f,
                _ => 1f
            };

            BossProjectileManager.SpawnProjectile(phase);
            ShotSound?.Play();
        }
    }
}
