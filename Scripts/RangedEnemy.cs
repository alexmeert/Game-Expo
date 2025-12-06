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

	private const string PERK_PATH = "res://Scenes/Items/Perks/";

	private NavigationAgent2D agent;

	protected Node2D TargetPlayer { get; private set; }
	private float _attackTimer = 0f;
	private float _pathUpdateCooldown = 0f;
	private const float PATH_UPDATE_INTERVAL = 0.3f; // Update path every 0.3 seconds max
	private const float TARGET_UPDATE_THRESHOLD = 50f; // Only update if target moved 50+ units
	private Vector2 _cachedFleeTarget = Vector2.Zero;
	private bool _hasCachedFleeTarget = false;

	public override void _Ready()
	{
		base._Ready();
		agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
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
	    Vector2 playerDir = (TargetPlayer.GlobalPosition - GlobalPosition).Normalized();
	
	    Vector2 desiredTarget;
	    if (distance < MinDistance)
	    {
	        // Flee - cache the target to avoid constant recalculation
	        if (!_hasCachedFleeTarget || _cachedFleeTarget.DistanceTo(GlobalPosition) < 50f)
	        {
	            _cachedFleeTarget = GlobalPosition - playerDir * 200f;
	            _hasCachedFleeTarget = true;
	        }
	        desiredTarget = _cachedFleeTarget;
	    }
	    else if (distance > MaxDistance)
	    {
	        // Approach
	        desiredTarget = TargetPlayer.GlobalPosition;
	        _hasCachedFleeTarget = false; // Reset flee cache when not fleeing
	    }
	    else
	    {
	        // Strafing
	        Vector2 tangent = new Vector2(-playerDir.Y, playerDir.X);
	        Velocity = tangent * (Spd * 0.4f);
	        UpdateAnimation();
	        _hasCachedFleeTarget = false; // Reset flee cache when strafing
	        return; // Skip agent movement
	    }
	
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
	
	    // Skip if path finished or invalid
	    if (agent.IsNavigationFinished())
	    {
	        Velocity = Vector2.Zero;
	    }
	    else
	    {
	        Vector2 nextPosition = agent.GetNextPathPosition();
	        Vector2 newVelocity = (nextPosition - GlobalPosition).Normalized() * Spd;
	
	        if (agent.AvoidanceEnabled)
	            agent.SetVelocity(newVelocity);
	        else
	            Velocity = newVelocity;
	    }
	
	    // Apply movement
	    MoveAndSlide();
	
	    // Attack timer
	    _attackTimer -= (float)delta;
	    if (_attackTimer <= 0f && distance <= AttackRange)
	    {
	        AttackPlayer();
	        _attackTimer = 1f / ATKSPD;
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
		if (ProjectileScene == null || FirePoint == null)
			return;

		if (TargetPlayer is not Player player || !player.IsAlive)
			return;

		Vector2 direction = (player.GlobalPosition - FirePoint.GlobalPosition).Normalized();

		var projectile = ProjectileScene.Instantiate<BasicProjectile>();
		projectile.GlobalPosition = FirePoint.GlobalPosition;
		projectile.SetDirection(direction);
		projectile.Owner = this;
		projectile.SetDamage(DMG);

		GetTree().CurrentScene.AddChild(projectile);

		ShotSound?.Play();
	}

	protected void FindPlayer()
	{
		var scene = GetTree().CurrentScene;
		if (scene == null)
			return;

		TargetPlayer = scene.GetChildren()
			.OfType<Player>()
			.FirstOrDefault(p => p.IsAlive);
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

		GD.Print("Ranged spawned perk: " + perkFile);
	}
}
