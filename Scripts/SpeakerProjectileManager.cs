using Godot;

public partial class SpeakerProjectileManager : Node
{
    private static readonly PackedScene SpeakerProjectile = GD.Load<PackedScene>("res://Scenes/Projectiles/Enemy/Speaker/SpeakerProjectile.tscn");

    public BasicEntity Owner { get; set; }

    public void SpawnTargetedProjectile(Vector2 spawnPosition, Vector2 targetPosition)
    {
        if (Owner == null || SpeakerProjectile == null) return;

        var node = SpeakerProjectile.Instantiate();
        if (node == null) return;

        // The scene root must be a BasicProjectile (Area2D with BasicProjectile script)
        var proj = node as BasicProjectile;
        if (proj == null)
        {
            GD.PrintErr($"SpeakerProjectileManager: Scene root must be a BasicProjectile! Current root type: {node.GetType().Name}. Please attach the BasicProjectile script to the root Area2D node in the scene.");
            node.QueueFree();
            return;
        }

        Vector2 direction = (targetPosition - spawnPosition).Normalized();
        
        proj.GlobalPosition = spawnPosition;
        proj.SetDirection(direction);
        proj.Owner = Owner;
        proj.SetDamage(Owner.DMG);

        GetTree().CurrentScene.AddChild(proj);
    }

    public void SpawnArcProjectiles(Vector2 spawnPosition, Vector2 targetPosition, float arcAngle = 45f)
    {
        if (Owner == null || SpeakerProjectile == null) return;

        // Calculate base direction to target
        Vector2 baseDirection = (targetPosition - spawnPosition).Normalized();
        float baseAngle = baseDirection.Angle();

        // Spawn 5 projectiles
        int projectileCount = 5;
        float angleStep = Mathf.DegToRad(arcAngle) / (projectileCount - 1);
        float startAngle = baseAngle - Mathf.DegToRad(arcAngle) / 2f;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            var node = SpeakerProjectile.Instantiate();
            if (node == null) continue;

            // The scene root must be a BasicProjectile (Area2D with BasicProjectile script)
            var proj = node as BasicProjectile;
            if (proj == null)
            {
                GD.PrintErr($"SpeakerProjectileManager: Scene root must be a BasicProjectile! Current root type: {node.GetType().Name}. Please attach the BasicProjectile script to the root Area2D node in the scene.");
                node.QueueFree();
                continue;
            }

            proj.GlobalPosition = spawnPosition;
            proj.SetDirection(direction);
            proj.Owner = Owner;
            proj.SetDamage(Owner.DMG);

            GetTree().CurrentScene.AddChild(proj);
        }
    }
}

