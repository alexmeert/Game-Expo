using Godot;
using System;
using System.Linq;

public partial class RangedEnemy : BasicEntity
{
	[Export] private int Hp = 30;
	[Export] private int Dmg = 5;
	[Export] private float AtkSpd = 1.5f;
	[Export] private float Def = 0f; // Percentage (0.0 = 0%, 1.0 = 100% damage reduction)
	[Export] private float Spd = 50; // Fast movement
	[Export] private AudioStreamPlayer2D HitSound;
	[Export] private AudioStreamPlayer2D ShotSound;
	[Export] private PackedScene ProjectileScene;
	[Export] private Marker2D FirePoint;
	[Export] private float MinDistance = 150f; // Minimum distance to keep from player
	[Export] private float MaxDistance = 400f; // Maximum distance before moving closer
	[Export] private float AttackRange = 500f; // Range at which enemy can attack

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
		Vector2 direction = (TargetPlayer.GlobalPosition - GlobalPosition).Normalized();

		// Keep distance from player - move away if too close, move closer if too far
		if (distance < MinDistance)
		{
			// Too close - move away from player
			Velocity = -direction * SPD;
		}
		else if (distance > MaxDistance)
		{
			// Too far - move closer to player (but slower)
			Velocity = direction * SPD * 0.6f;
		}
		else
		{
			// In optimal range - strafe or stop
			// Strafe perpendicular to player direction
			Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
			Velocity = perpendicular * SPD * 0.4f;
		}

		// Attack logic
		_attackTimer -= (float)delta;
		if (_attackTimer <= 0f && distance <= AttackRange)
		{
			AttackPlayer();
			_attackTimer = 1.0f / ATKSPD;
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
		if (TargetPlayer == null || ProjectileScene == null || FirePoint == null)
			return;

		if (TargetPlayer is Player player && player.IsAlive)
		{
			// Calculate direction to player
			Vector2 direction = (player.GlobalPosition - FirePoint.GlobalPosition).Normalized();

			// Spawn projectile
			var projectile = ProjectileScene.Instantiate<BasicProjectile>();
			projectile.GlobalPosition = FirePoint.GlobalPosition;
			projectile.SetDirection(direction);
			projectile.Owner = this;
			projectile.SetDamage(DMG);

			// Add to scene
			GetTree().CurrentScene.AddChild(projectile);

			// Play shot sound
			if (ShotSound != null)
			{
				ShotSound.Play();
			}

			GD.Print($"RangedEnemy shot at player. Projectile damage: {DMG}");
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
