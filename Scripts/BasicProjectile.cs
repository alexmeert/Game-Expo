using Godot;

public partial class BasicProjectile : Area2D
{
	[Export] public float SPD = 400f;
	[Export] public float DMG = 10f;

	public Vector2 Direction = Vector2.Up;
	public BasicEntity Owner { get; set; }

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		Position += Direction * SPD * (float)delta;
	}

	private void OnBodyEntered(Node body)
	{
		if (body == Owner)
			return;

		if (Owner is RangedEnemy or MeleeEnemy or Speaker)
		{
			if (body is Player player && player.IsAlive)
			{
				player.TakeDamage(DMG);
				QueueFree();
			}
			return;
		}

		if (Owner is Player)
		{
			if (body is BasicEntity entity && entity != Owner && entity is not Player)
			{
				entity.TakeDamage(DMG);
				QueueFree();
			}
		}
	}

	public void SetDirection(Vector2 dir)
	{
		Direction = dir.Normalized();
	}

	public void SetDamage(float dmg)
	{
		DMG = dmg;
	}
}
