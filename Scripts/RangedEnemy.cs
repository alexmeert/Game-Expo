using Godot;
using System;
using System.Linq;

public partial class RangedEnemy : BasicEntity
{
	


	[Export] private int Hp = 30;
	[Export] private int Dmg = 5;
	[Export] private float AtkSpd = 1.5f;
	[Export] private float Def = 0f;
	[Export] private float Spd = 50;
	[Export] private AudioStreamPlayer2D HitSound;
	[Export] private AudioStreamPlayer2D ShotSound;
	[Export] private AudioStreamPlayer2D DeathSound;
	[Export] private PackedScene ProjectileScene;
	[Export] private Marker2D FirePoint;
	[Export] private float MinDistance = 150f;
	[Export] private float MaxDistance = 400f;
	[Export] private float AttackRange = 500f;

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

		if (distance < MinDistance)
		{
			Velocity = -direction * SPD;
		}
		else if (distance > MaxDistance)
		{
			Velocity = direction * SPD * 0.6f;
		}
		else
		{
			Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
			Velocity = perpendicular * SPD * 0.4f;
		}

		_attackTimer -= (float)delta;
		if (_attackTimer <= 0f && distance <= AttackRange)
		{
			AttackPlayer();
			_attackTimer = 1.0f / ATKSPD;
		}

		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		var animSprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");
		if (animSprite == null)
			return;

		if (Velocity.LengthSquared() < 0.01f)
		{
			return;
		}

		Vector2 normalizedVel = Velocity.Normalized();
		
		if (Mathf.Abs(normalizedVel.X) > Mathf.Abs(normalizedVel.Y))
		{
			animSprite.Play(normalizedVel.X > 0 ? "WalkRight" : "WalkLeft");
		}
		else
		{
			animSprite.Play(normalizedVel.Y > 0 ? "WalkDown" : "WalkUp");
		}
	}

	private void AttackPlayer()
	{
		if (TargetPlayer == null || ProjectileScene == null || FirePoint == null)
			return;

		if (TargetPlayer is Player player && player.IsAlive)
		{
			Vector2 direction = (player.GlobalPosition - FirePoint.GlobalPosition).Normalized();

			var projectile = ProjectileScene.Instantiate<BasicProjectile>();
			projectile.GlobalPosition = FirePoint.GlobalPosition;
			projectile.SetDirection(direction);
			projectile.Owner = this;
			projectile.SetDamage(DMG);

			GetTree().CurrentScene.AddChild(projectile);

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
			TargetPlayer = scene.GetNodeOrNull<Node2D>("MainCharacter");
		}
	}

	protected override void OnTakeDamage(float damage)
	{
		base.OnTakeDamage(damage);
		if (HitSound != null)
		{
			HitSound.Play();
		}
	}
	
    protected override void Die()
    {
        if (DeathSound != null)
        {
            DeathSound.Reparent(GetTree().CurrentScene);
            DeathSound.Play();
        }
        EmitSignal(SignalName.EnemyDied);
        base.Die();
    }

}
