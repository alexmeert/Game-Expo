using Godot;

public partial class BossProjectileManager : Node
{
    private static readonly PackedScene Phase1Proj = GD.Load<PackedScene>("res://Scenes/Projectiles/Boss/Phase1.tscn");
    private static readonly PackedScene Phase2Proj = GD.Load<PackedScene>("res://Scenes/Projectiles/Boss/Phase2.tscn");
    private static readonly PackedScene Phase3Proj = GD.Load<PackedScene>("res://Scenes/Projectiles/Boss/Phase3.tscn");

    public BasicEntity Owner { get; set; }

    public void SpawnProjectile(int phase)
    {
        if (Owner == null) return;

        PackedScene scene = phase switch
        {
            1 => Phase1Proj,
            2 => Phase2Proj,
            3 => Phase3Proj,
            _ => null
        };
        if (scene == null) return;

        var proj = scene.Instantiate<BasicProjectile>();
        proj.GlobalPosition = Owner.GlobalPosition;
        proj.SetDirection(Vector2.Right);
        proj.Owner = Owner;
        proj.SetDamage(Owner.DMG);

        GetTree().CurrentScene.AddChild(proj);
    }
}
