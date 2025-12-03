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
	[Export] private AudioStreamPlayer2D HitSound;
	[Export] private float AttackRange = 35f;

	private const string PERK_PATH = "res://Scenes/Items/Perks/";

    protected override void HandleMovement(double delta)
    {
        if (TargetPlayer == null || !TargetPlayer.IsInsideTree())
        {
            FindPlayer();
            if (TargetPlayer == null) return;
        }

        float distance = GlobalPosition.DistanceTo(TargetPlayer.GlobalPosition);

		SetStats(Hp, Dmg, AtkSpd, Def, Spd);
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
		    Velocity = direction * Spd;

		    if (!WalkSound.Playing)
		        WalkSound.Play();
		}
		else
			Velocity = Vector2.Zero;

		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		var anim = GetNodeOrNull<AnimatedSprite2D>("Sprite");
		if (anim == null)
			return;

		if (Velocity.LengthSquared() < 0.01f)
			return;

		var dir = Velocity.Normalized();
		if (Mathf.Abs(dir.X) > Mathf.Abs(dir.Y))
			anim.Play(dir.X > 0 ? "WalkRight" : "WalkLeft");
		else
			anim.Play(dir.Y > 0 ? "WalkDown" : "WalkUp");
	}

	private void AttackPlayer()
	{
		if (TargetPlayer is Player p && p.IsAlive)
			p.TakeDamage(DMG);
	}

	protected virtual void FindPlayer()
	{
		var scene = GetTree().CurrentScene;
		if (scene != null)
		{
			TargetPlayer = scene.GetNodeOrNull<Node2D>("MainCharacter")
				?? scene.GetNodeOrNull<Player>("Player")
				?? scene.GetChildren().OfType<Player>().FirstOrDefault();
		}
	}

	protected override void OnTakeDamage(float damage)
	{
		base.OnTakeDamage(damage);
		HitSound?.Play();
	}

	protected override void Die()
	{
		EmitSignal(SignalName.EnemyDied);
		TrySpawnPerk();
		base.Die();
	}

	
	//       PERK DROP SYSTEM
	
	private void TrySpawnPerk()
	{
		Random random = new Random();
		float roll = (float)random.NextDouble();

		if (roll > 0.05f)  // 5% chance
			return;

		string[] perks = DirAccess.GetFilesAt(PERK_PATH);

		if (perks.Length == 0)
		{
			GD.PushWarning("No perks found in: " + PERK_PATH);
			return;
		}

		string perkFile = perks[random.Next(perks.Length)];
		PackedScene scene = GD.Load<PackedScene>(PERK_PATH + perkFile);
		if (scene == null)
		{
			GD.PushError("Failed to load perk: " + perkFile);
			return;
		}

		Node2D perkInstance = scene.Instantiate<Node2D>();
		perkInstance.GlobalPosition = GlobalPosition;

		// Use deferred add to avoid "flushing queries" error
		GetTree().CurrentScene.CallDeferred("add_child", perkInstance);

		GD.Print("Tank spawned perk: " + perkFile);
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
