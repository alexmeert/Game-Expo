using Godot;

public partial class BossProjectileManager : Node
{
    private static readonly PackedScene Phase1Proj = GD.Load<PackedScene>("res://Scenes/Projectiles/Enemy/Boss/BossProjectile1.tscn");
    private static readonly PackedScene Phase2Proj = GD.Load<PackedScene>("res://Scenes/Projectiles/Enemy/Boss/BossProjectile2.tscn");

    public BasicEntity Owner { get; set; }

    private Player targetPlayer;

    public override void _Ready()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        var scene = GetTree().CurrentScene;
        if (scene != null)
        {
            var mainCharacter = scene.GetNodeOrNull<CharacterBody2D>("MainCharacter");
            if (mainCharacter is Player player)
            {
                targetPlayer = player;
            }
        }
    }

    public void SpawnPhase1Projectile(Vector2 spawnPosition)
    {
        if (Owner == null || Phase1Proj == null) return;

        if (targetPlayer == null || !targetPlayer.IsAlive)
        {
            FindPlayer();
            if (targetPlayer == null) return;
        }

        Vector2 direction = (targetPlayer.GlobalPosition - spawnPosition).Normalized();

        var proj = Phase1Proj.Instantiate<BasicProjectile>();
        proj.GlobalPosition = spawnPosition;
        proj.SetDirection(direction);
        proj.Owner = Owner;
        proj.SetDamage(Owner.DMG);

        GetTree().CurrentScene.AddChild(proj);
    }

    /// <summary>
    /// Phase 2: 5 projectiles in arc pattern (like right speaker)
    /// </summary>
    public void SpawnPhase2Projectiles(Vector2 spawnPosition, float arcAngle = 45f)
    {
        if (Owner == null || Phase2Proj == null) return;

        if (targetPlayer == null || !targetPlayer.IsAlive)
        {
            FindPlayer();
            if (targetPlayer == null) return;
        }

        // Calculate base direction to target
        Vector2 baseDirection = (targetPlayer.GlobalPosition - spawnPosition).Normalized();
        float baseAngle = baseDirection.Angle();

        // Spawn 5 projectiles
        int projectileCount = 5;
        float angleStep = Mathf.DegToRad(arcAngle) / (projectileCount - 1);
        float startAngle = baseAngle - Mathf.DegToRad(arcAngle) / 2f;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            var proj = Phase2Proj.Instantiate<BasicProjectile>();
            proj.GlobalPosition = spawnPosition;
            proj.SetDirection(direction);
            proj.Owner = Owner;
            proj.SetDamage(Owner.DMG);

            GetTree().CurrentScene.AddChild(proj);
        }
    }

    /// <summary>
    /// Phase 3: Both attack patterns combined
    /// </summary>
    public void SpawnPhase3Projectiles(Vector2 spawnPosition, float arcAngle = 45f)
    {
        // Spawn both phase 1 and phase 2 attacks
        SpawnPhase1Projectile(spawnPosition);
        SpawnPhase2Projectiles(spawnPosition, arcAngle);
    }
}
