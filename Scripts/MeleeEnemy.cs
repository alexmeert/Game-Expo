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
	[Export] private float AttackRange = 30f;

	// Folder containing perks
	private const string PERK_PATH = "res://Scenes/Items/Perks/";

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
			return;

		Vector2 dir = Velocity.Normalized();

		if (Mathf.Abs(dir.X) > Mathf.Abs(dir.Y))
			animSprite.Play(dir.X > 0 ? "WalkRight" : "WalkLeft");
		else
			animSprite.Play(dir.Y > 0 ? "WalkDown" : "WalkUp");
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

		base.Die(); // Calls QueueFree()
	}

	
	//      PERK DROP SYSTEM
	
	private void TrySpawnPerk()
	{
		Random random = new Random();
		float roll = (float)random.NextDouble();

		if (roll > 0.05f) 
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

		
		GetTree().CurrentScene.CallDeferred("add_child", perkInstance);

		GD.Print("Spawned perk: " + perkFile);
	}

}
