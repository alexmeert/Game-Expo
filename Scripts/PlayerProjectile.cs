using Godot;

public partial class PlayerProjectile : Node
{
    private static readonly PackedScene Projectile0Scene = GD.Load<PackedScene>("res://Scenes/Projectiles/Projectile0.tscn");
    private static readonly PackedScene Projectile1Scene = GD.Load<PackedScene>("res://Scenes/Projectiles/Projectile1.tscn");

    private bool _useFirstProjectile = true;

    public BasicEntity Owner { get; set; }

    public void SpawnProjectile(Vector2 position, float rotation)
    {
        var projectileScene = _useFirstProjectile ? Projectile0Scene : Projectile1Scene;
        _useFirstProjectile = !_useFirstProjectile;

        var projectile = projectileScene.Instantiate<BasicProjectile>();

        projectile.GlobalPosition = position;

        projectile.SetDirection(Vector2.Right.Rotated(rotation));

        projectile.Owner = Owner;
        projectile.SetDamage(Owner.DMG);

        GetTree().CurrentScene.AddChild(projectile);
    }

}