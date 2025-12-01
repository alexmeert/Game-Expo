using Godot;

public partial class PlayerProjectile : Node
{
	private static readonly PackedScene Projectile0Scene = GD.Load<PackedScene>("res://Scenes/Projectiles/Player/Projectile0.tscn");
	private static readonly PackedScene Projectile1Scene = GD.Load<PackedScene>("res://Scenes/Projectiles/Player/Projectile1.tscn");

	private bool _useFirstProjectile = true;

	public BasicEntity Owner { get; set; }

	public void SpawnProjectile(Vector2 position, float rotation)
	{
		if (Owner == null)
		{
			return;
		}

		var projectileScene = _useFirstProjectile ? Projectile0Scene : Projectile1Scene;
		_useFirstProjectile = !_useFirstProjectile;

		if (projectileScene == null)
		{
			return;
		}

		var projectile = projectileScene.Instantiate<BasicProjectile>();
		if (projectile == null)
		{
			return;
		}

		projectile.GlobalPosition = position;
		projectile.SetDirection(Vector2.Right.Rotated(rotation));
		projectile.Owner = Owner;
		projectile.SetDamage(Owner.DMG);

		var scene = GetTree().CurrentScene;
		if (scene == null)
		{
			return;
		}

		scene.AddChild(projectile);
	}

}
