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
		AreaEntered += OnAreaEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		Position += Direction * SPD * (float)delta;
	}

	private void OnBodyEntered(Node body)
	{
		HandleCollision(body);
	}

	private void OnAreaEntered(Area2D area)
	{
		HandleCollision(area);
	}

	private void HandleCollision(Node body)
	{
		if (body == Owner)
			return;

		// Handle enemy projectiles (RangedEnemy, MeleeEnemy, Speaker, or Boss)
		if (Owner is RangedEnemy or MeleeEnemy or Speaker or Boss)
		{
			// For enemy projectiles, check if body is Player
			if (body is Player player && player.IsAlive)
			{
				GD.Print($"Projectile from {Owner.GetType().Name} hit Player for {DMG} damage");
				player.TakeDamage(DMG);
				QueueFree();
			}
			return;
		}

		// Handle player projectiles
		if (Owner is Player)
		{
			// Find the actual entity (Boss or other enemy) - check body and its parents
			BasicEntity targetEntity = FindEntityFromNode(body);
			
			// Debug: log what we found
			if (targetEntity == null)
			{
				var groups = body.GetGroups();
				var groupList = new System.Collections.Generic.List<string>();
				foreach (StringName group in groups)
				{
					groupList.Add(group.ToString());
				}
			}
			
			if (targetEntity == null || targetEntity == Owner || targetEntity is Player)
				return;

			// Handle Boss explicitly
			if (targetEntity is Boss boss && boss.IsAlive)
			{
				boss.TakeDamage(DMG);
				QueueFree();
				return;
			}

			// Handle other BasicEntity enemies
			if (targetEntity.IsAlive)
			{
				targetEntity.TakeDamage(DMG);
				QueueFree();
			}
		}
	}

	/// <summary>
	/// Finds the BasicEntity from a node, checking the node itself and traversing up parents.
	/// This handles cases where the collision body is a child node (like a hitbox in a group).
	/// </summary>
	private BasicEntity FindEntityFromNode(Node node)
	{
		if (node == null)
			return null;

		// Check if the node itself is a BasicEntity
		if (node is BasicEntity entity)
			return entity;

		// Always traverse up the parent tree to find any BasicEntity
		// This handles cases where the collision body is a child node (hitbox, collision shape, etc.)
		Node current = node;
		while (current != null)
		{
			// Check current node
			if (current is BasicEntity basicEntity)
				return basicEntity;
			
			// Move to parent
			current = current.GetParent();
			
			// Safety check to avoid infinite loops
			if (current == node)
				break;
		}

		return null;
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
