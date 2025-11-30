using Godot;
using System;

public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene EnemyScene;
	[Export] public Node LevelController;
	[Export] public int SpawnCount = 1;
	[Export] public float InitialDelay = 0f;
	[Export] public float SpawnRadius = 150f;
	[Export] public float DelayBetweenSpawns = 0.1f;

	private Timer _delayTimer;
	private Timer _spawnTimer;
	private int _spawnedCount = 0;

	public override void _Ready()
	{
		if (InitialDelay > 0)
		{
			_delayTimer = new Timer { OneShot = true, WaitTime = InitialDelay };
			_delayTimer.Timeout += StartSpawning;
			AddChild(_delayTimer);
			_delayTimer.Start();
		}
		else
			StartSpawning();
	}

	private void StartSpawning()
	{
		if (DelayBetweenSpawns <= 0.01f)
		{
			for (int i = 0; i < SpawnCount; i++)
				SpawnEnemy();
		}
		else
		{
			_spawnTimer = new Timer { WaitTime = DelayBetweenSpawns };
			_spawnTimer.Timeout += SpawnSingleEnemy;
			AddChild(_spawnTimer);

			SpawnSingleEnemy(); // Spawn immediately
			_spawnTimer.Start();
		}
	}

	private void SpawnSingleEnemy()
	{
		SpawnEnemy();
		_spawnedCount++;

		if (_spawnedCount >= SpawnCount)
		{
			_spawnTimer.Stop();
			_spawnTimer.QueueFree();
		}
	}

	private void SpawnEnemy()
	{
		if (EnemyScene == null) return;

		var enemy = EnemyScene.Instantiate<Node2D>();

		float angle = (float)GD.RandRange(0, Mathf.Tau);
		float dist = (float)GD.RandRange(0, SpawnRadius);

		enemy.GlobalPosition = GlobalPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

		GetParent().AddChild(enemy);

		// Connect death tracking
		if (enemy is MeleeEnemy melee)
		{
			melee.EnemyDied += OnEnemyDied;

			if (LevelController is IEnemyTracker t)
				t.OnEnemySpawned();
		}
	}

	private void OnEnemyDied()
	{
		if (LevelController is IEnemyTracker t)
			t.OnEnemyDied();
	}
}
