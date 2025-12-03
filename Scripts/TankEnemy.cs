using Godot;
using System;
using System.Linq;

public partial class TankEnemy : BasicEntity
{
    [Export] private int Hp = 200;
    [Export] private int Dmg = 15;
    [Export] private float AtkSpd = 0.4f;
    [Export] private float Def = 0.1f;
    [Export] private float Spd = 15;
    [Export] private float AttackRange = 35f;
    [Export] private AudioStreamPlayer2D HitSound;
    [Export] private AudioStreamPlayer2D DeathSound;
	[Export] private AudioStreamPlayer2D WalkSound;

    protected Node2D TargetPlayer { get; private set; }
    private float _attackTimer = 0f;

    protected override void InitializeEntity()
    {
        base.InitializeEntity();
        SetStats(hp: Hp, dmg: Dmg, atkspd: AtkSpd, def: Def, spd: Spd);
        FindPlayer();
    }

    protected override void HandleMovement(double delta)
    {
        if (TargetPlayer == null || !TargetPlayer.IsInsideTree())
        {
            FindPlayer();
            if (TargetPlayer == null) return;
        }

        float distance = GlobalPosition.DistanceTo(TargetPlayer.GlobalPosition);

        _attackTimer -= (float)delta;
        if (_attackTimer <= 0f && distance <= AttackRange)
        {
            AttackPlayer();
            _attackTimer = 1.0f / AtkSpd;
        }

		if (distance > AttackRange)
		{
		    Vector2 direction = (TargetPlayer.GlobalPosition - GlobalPosition).Normalized();
		    Velocity = direction * Spd;

		    if (!WalkSound.Playing)
		        WalkSound.Play();
		}
		else
		{
		    Velocity = Vector2.Zero;
		    WalkSound.Stop();
		}

        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        var animSprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");
        if (animSprite == null) return;
        if (Velocity.LengthSquared() < 0.01f) return;

        Vector2 v = Velocity.Normalized();
        if (Mathf.Abs(v.X) > Mathf.Abs(v.Y))
            animSprite.Play(v.X > 0 ? "WalkRight" : "WalkLeft");
        else
            animSprite.Play(v.Y > 0 ? "WalkDown" : "WalkUp");
    }

    private void AttackPlayer()
    {
        if (TargetPlayer is Player player && player.IsAlive)
        {
            player.TakeDamage(Dmg);
        }
    }

    protected virtual void FindPlayer()
    {
        var scene = GetTree().CurrentScene;
        if (scene == null) return;

        TargetPlayer = scene.GetNodeOrNull<Node2D>("MainCharacter")
            ?? scene.GetNodeOrNull<Player>("Player")
            ?? scene.GetChildren().OfType<Player>().FirstOrDefault();
    }

    protected override void OnTakeDamage(float damage)
    {
        base.OnTakeDamage(damage);
        if (HitSound != null) HitSound.Play();
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
