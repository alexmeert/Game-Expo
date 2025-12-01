using Godot;
using System;
using System.Linq;

public partial class TankEnemy : BasicEntity
{
	[Export] private int Hp = 200; // High health
	[Export] private int Dmg = 15; // High damage
	[Export] private float AtkSpd = 0.4f; // Much slower attack speed (0.4 attacks/sec = 2.5 sec between attacks)
	[Export] private float Def = 0.1f; // 10% damage reduction
	[Export] private float Spd = 15; // Slower than MeleeEnemy (20)
	[Export] private AudioStreamPlayer2D HitSound;
	[Export] private float AttackRange = 35f; // Slightly larger attack range

	protected Node2D TargetPlayer { get; private set; }
	private float _attackTimer = 0f;

	protected override void InitializeEntity()
	{
		base.InitializeEntity();

		SetStats(
			hp: Hp,
			dmg: Dmg,
			atkspd: AtkSpd,
			def: Def,
			spd: Spd
		);

		FindPlayer();
	}

	protected override void HandleMovement(double delta)
	{
		if (TargetPlayer == null || !TargetPlayer.IsInsideTree())
		{
			FindPlayer();
			if (TargetPlayer == null)
				return;
		}

		float distance = GlobalPosition.DistanceTo(TargetPlayer.GlobalPosition);
		
		// Attack logic
		_attackTimer -= (float)delta;
		if (_attackTimer <= 0f && distance <= AttackRange)
		{
			AttackPlayer();
			_attackTimer = 1.0f / ATKSPD;
		}
		
		// Move toward player if not in attack range
		if (distance > AttackRange)
		{
			Vector2 direction = (TargetPlayer.GlobalPosition - GlobalPosition).Normalized();
			Velocity = direction * SPD;
		}
		else
		{
			// Stop moving when in attack range
			Velocity = Vector2.Zero;
		}

		// Handle animation
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		var animSprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");
		if (animSprite == null)
			return;

		if (Velocity.LengthSquared() < 0.01f)
		{
			// Not moving - keep current animation or play idle if available
			return;
		}

		// Determine animation based on velocity direction
		Vector2 normalizedVel = Velocity.Normalized();
		
		if (Mathf.Abs(normalizedVel.X) > Mathf.Abs(normalizedVel.Y))
		{
			// Moving horizontally
			animSprite.Play(normalizedVel.X > 0 ? "WalkRight" : "WalkLeft");
		}
		else
		{
			// Moving vertically
			animSprite.Play(normalizedVel.Y > 0 ? "WalkDown" : "WalkUp");
		}
	}

	private void AttackPlayer()
	{
		if (TargetPlayer is Player player && player.IsAlive)
		{
			player.TakeDamage(DMG);
			GD.Print($"TankEnemy dealt {DMG} damage to player. Player HP: {player.HP}/{player.MaxHP}");
		}
	}

	protected virtual void FindPlayer()
	{
		var scene = GetTree().CurrentScene;
		if (scene != null)
		{
			// Try to find player by name first
			TargetPlayer = scene.GetNodeOrNull<Node2D>("MainCharacter");
			
			// If not found, search for Player node by type
			if (TargetPlayer == null)
			{
				TargetPlayer = scene.GetNodeOrNull<Player>("Player");
			}
			
			// Last resort: search all children for Player type
			if (TargetPlayer == null)
			{
				var players = scene.GetChildren().OfType<Player>();
				if (players.Any())
				{
					TargetPlayer = players.First();
				}
			}
		}
	}

	protected override void OnTakeDamage(float damage)
	{
		base.OnTakeDamage(damage);
		
		// Play hit sound when enemy is hit by a projectile
		if (HitSound != null)
		{
			HitSound.Play();
		}
	}
}
