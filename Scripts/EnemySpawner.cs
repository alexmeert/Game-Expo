using Godot;
using System;

public partial class EnemySpawner : Node2D
{
    [Export] public PackedScene EnemyScene;
    [Export] public int SpawnCount = 1;
    [Export] public float InitialDelay = 0f;
    [Export] public float SpawnRadius = 150f;
    [Export] public float DelayBetweenSpawns = 0.1f;

    private Timer _delayTimer;
    private Timer _spawnTimer;
    private int _spawnedCount = 0;
    private bool _hasStarted = false;

    public override void _Ready()
    {
        base._Ready();

        // Create and setup initial delay timer
        if (InitialDelay > 0f)
        {
            _delayTimer = new Timer();
            _delayTimer.WaitTime = InitialDelay;
            _delayTimer.OneShot = true;
            _delayTimer.Timeout += OnInitialDelayComplete;
            AddChild(_delayTimer);
            _delayTimer.Start();
        }
        else
        {
            // No delay, start spawning immediately
            StartSpawning();
        }
    }

    private void OnInitialDelayComplete()
    {
        StartSpawning();
    }

    private void StartSpawning()
    {
        if (_hasStarted)
            return;

        _hasStarted = true;

        // If delay between spawns is 0 or very small, spawn all at once
        if (DelayBetweenSpawns <= 0.01f)
        {
            SpawnAllEnemies();
        }
        else
        {
            // Spawn enemies one by one with delay
            _spawnTimer = new Timer();
            _spawnTimer.WaitTime = DelayBetweenSpawns;
            _spawnTimer.Timeout += SpawnSingleEnemy;
            AddChild(_spawnTimer);
            _spawnTimer.Start();
            
            // Spawn first enemy immediately
            SpawnSingleEnemy();
        }
    }

    private void SpawnAllEnemies()
    {
        for (int i = 0; i < SpawnCount; i++)
        {
            SpawnEnemy();
        }
    }

    private void SpawnSingleEnemy()
    {
        SpawnEnemy();
        _spawnedCount++;

        // Stop timer if we've spawned all enemies
        if (_spawnedCount >= SpawnCount)
        {
            if (_spawnTimer != null)
            {
                _spawnTimer.Stop();
                _spawnTimer.QueueFree();
                _spawnTimer = null;
            }
        }
    }

    private void SpawnEnemy()
    {
        var enemy = EnemyScene.Instantiate<Node2D>();

        // Calculate random position within spawn radius
        Vector2 spawnPosition = GlobalPosition;
        if (SpawnRadius > 0f)
        {
            float angle = (float)GD.RandRange(0, Mathf.Tau);
            float distance = (float)GD.RandRange(0, SpawnRadius);
            spawnPosition += new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        }

        enemy.GlobalPosition = spawnPosition;

        var parent = GetParent();
        if (parent != null)
        {
            parent.AddChild(enemy);
        }
        else
        {
            enemy.QueueFree();
        }
    }
}
