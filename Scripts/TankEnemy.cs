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
	[Export] private AudioStreamPlayer2D WalkSound;
	[Export] private AudioStreamPlayer2D DeathSound;

	private NavigationAgent2D agent;

	private float _attackTimer = 0f;
	private const string PERK_PATH = "res://Scenes/Items/Perks/";
	private float _pathUpdateCooldown = 0f;
	private const float PATH_UPDATE_INTERVAL = 0.3f; // Update path every 0.3 seconds max
	private const float TARGET_UPDATE_THRESHOLD = 50f; // Only update if target moved 50+ units
	
	// Stuck detection
	private Vector2 _lastPosition = Vector2.Zero;
	private float _stuckTimer = 0f;
	private const float STUCK_CHECK_INTERVAL = 0.5f; // Check if stuck every 0.5 seconds
	private const float STUCK_THRESHOLD = 5f; // If moved less than 5 units, consider stuck
	private bool _useDirectPath = false;

	protected Node2D TargetPlayer { get; private set; }

	public override void _Ready()
	{
		base._Ready();
		agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
		_lastPosition = GlobalPosition;
		
		// Wait for navigation to be ready
		if (agent != null)
		{
			agent.TargetDesiredDistance = 5f;
			agent.PathDesiredDistance = 5f;
			agent.PathMaxDistance = 3.5f;
		}
	}

	protected override void InitializeEntity()
	{
		base.InitializeEntity();

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
	
		// Attack
		_attackTimer -= (float)delta;
		if (_attackTimer <= 0f && distance <= AttackRange)
		{
			AttackPlayer();
			_attackTimer = 1.0f / ATKSPD;
		}
	
		// Movement
		if (distance > AttackRange)
		{
			Vector2 desiredTarget = TargetPlayer.GlobalPosition;
			
			// Check if stuck
			_stuckTimer -= (float)delta;
			if (_stuckTimer <= 0f)
			{
				float movedDistance = GlobalPosition.DistanceTo(_lastPosition);
				if (movedDistance < STUCK_THRESHOLD && Velocity.LengthSquared() > 0.01f)
				{
					// We're stuck, use direct path
					_useDirectPath = true;
				}
				else if (movedDistance > STUCK_THRESHOLD * 2f)
				{
					// We're moving well, try navigation again
					_useDirectPath = false;
				}
				_lastPosition = GlobalPosition;
				_stuckTimer = STUCK_CHECK_INTERVAL;
			}
			
			Vector2 newVelocity = Vector2.Zero;
			
			// Try navigation first if not stuck
			if (!_useDirectPath && agent != null)
			{
				// Update path only if:
				// 1. Cooldown has passed
				// 2. Target has moved significantly
				_pathUpdateCooldown -= (float)delta;
				if (_pathUpdateCooldown <= 0f)
				{
					float targetDistance = desiredTarget.DistanceTo(agent.TargetPosition);
					if (targetDistance > TARGET_UPDATE_THRESHOLD)
					{
						agent.TargetPosition = desiredTarget;
						_pathUpdateCooldown = PATH_UPDATE_INTERVAL;
					}
				}
	
				if (agent.IsNavigationFinished())
				{
					newVelocity = Vector2.Zero;
					WalkSound?.Stop();
				}
				else
				{
					Vector2 next = agent.GetNextPathPosition();
					newVelocity = (next - GlobalPosition).Normalized() * Spd;
					
					if (!WalkSound.Playing)
						WalkSound?.Play();
				}
			}
			
			// Fallback to direct path if navigation failed or we're stuck
			if (_useDirectPath || newVelocity.LengthSquared() < 0.01f)
			{
				Vector2 directDir = (desiredTarget - GlobalPosition).Normalized();
				newVelocity = directDir * Spd;
				
				// Simple obstacle avoidance: check if we can move directly
				// Use a raycast or space query to detect obstacles
				var spaceState = GetWorld2D().DirectSpaceState;
				var query = PhysicsRayQueryParameters2D.Create(GlobalPosition, GlobalPosition + directDir * 20f);
				query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };
				var result = spaceState.IntersectRay(query);
				
				if (result.Count > 0)
				{
					// Obstacle detected, try to go around it
					Vector2 obstacleNormal = ((Vector2)result["normal"]).Normalized();
					// Move perpendicular to the obstacle
					Vector2 avoidDir = new Vector2(-obstacleNormal.Y, obstacleNormal.X);
					newVelocity = avoidDir * Spd;
				}
				
				if (!WalkSound.Playing && newVelocity.LengthSquared() > 0.01f)
					WalkSound?.Play();
			}
			
			Velocity = newVelocity;
		}
		else
		{
			Velocity = Vector2.Zero;
			WalkSound?.Stop();
		}
	
		MoveAndSlide();
		UpdateAnimation();
	}


	private void UpdateAnimation()
	{
		var anim = GetNodeOrNull<AnimatedSprite2D>("Sprite");
		if (anim == null)
			return;

		if (Velocity.LengthSquared() < 0.01f)
			return;

		Vector2 dir = Velocity.Normalized();

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
			TargetPlayer =
				scene.GetNodeOrNull<Node2D>("MainCharacter") ??
				scene.GetNodeOrNull<Player>("Player") ??
				scene.GetChildren().OfType<Player>().FirstOrDefault();
			
			// Set initial navigation target
			if (TargetPlayer != null && agent != null)
			{
				agent.TargetPosition = TargetPlayer.GlobalPosition;
			}
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

		if (DeathSound != null)
		{
			DeathSound.Reparent(GetTree().CurrentScene);
			DeathSound.Play();
		}

		base.Die();
	}

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

		GD.Print($"Tank spawned perk: {perkFile}");
	}
}
