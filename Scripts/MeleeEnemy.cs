using Godot;
using System;
using System.Linq;

public partial class MeleeEnemy : BasicEntity
{	
	
	
	
	[Export] private int Hp = 50;
	[Export] private int Dmg = 1;
	[Export] private float AtkSpd = 1.2f;
	[Export] private float Def = 0f;
	[Export] private float Spd = 20;
	[Export] private AudioStreamPlayer2D HitSound;
	[Export] private AudioStreamPlayer2D DeathSound;
	[Export] private float AttackRange = 30f;
	


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
		
		_attackTimer -= (float)delta;
		if (_attackTimer <= 0f && distance <= AttackRange)
		{
			AttackPlayer();
			_attackTimer = 1.0f / ATKSPD;
		}
		
		if (distance > AttackRange)
		{
			Vector2 direction = (TargetPlayer.GlobalPosition - GlobalPosition).Normalized();
			Velocity = direction * SPD;
		}
		else
		{
			Velocity = Vector2.Zero;
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
		if (TargetPlayer is Player player && player.IsAlive)
		{
			player.TakeDamage(DMG);
			GD.Print($"MeleeEnemy dealt {DMG} damage to player. Player HP: {player.HP}/{player.MaxHP}");
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
			HitSound.Play();
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
