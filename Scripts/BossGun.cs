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
            Vector2 spawnPosition = Muzzle != null ? Muzzle.GlobalPosition : BossProjectileManager.Owner.GlobalPosition;

            switch (phase)
            {
                case 1:
                    // Phase 1: Single targeted projectile (like left speaker)
                    attackTimer = 1.2f;
                    BossProjectileManager.SpawnPhase1Projectile(spawnPosition);
                    break;
                case 2:
                    // Phase 2: 5 projectiles in arc (like right speaker)
                    attackTimer = 0.7f;
                    BossProjectileManager.SpawnPhase2Projectiles(spawnPosition, 45f);
                    break;
                case 3:
                    // Phase 3: Both attacks combined
                    attackTimer = 0.5f;
                    BossProjectileManager.SpawnPhase3Projectiles(spawnPosition, 45f);
                    break;
            }

            ShotSound?.Play();
        }
    }
}
