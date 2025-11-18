using Godot;

public partial class Spawner : Node2D
{
    [Export] public PackedScene MeleeEnemyScene;
    [Export] public Timer SpawnTimer;
    [Export] public float SpawnRadius = 150f;

    public override void _Ready()
    {
        base._Ready();
        
        if (SpawnTimer == null)
        {
            SpawnTimer = GetNodeOrNull<Timer>("Timer");
        }
        
        if (SpawnTimer != null)
        {
            SpawnTimer.Timeout += OnTimerTimeout;
            
            // Make sure timer is started
            if (SpawnTimer.IsStopped())
            {
                SpawnTimer.Start();
            }
        }
        else
        {
            GD.PrintErr("Spawner: No Timer found! Make sure to assign a Timer node.");
        }
        
        if (MeleeEnemyScene == null)
        {
            GD.PrintErr("Spawner: MeleeEnemyScene is not assigned!");
        }
    }

    private void OnTimerTimeout()
    {
        if (MeleeEnemyScene == null)
        {
            GD.PrintErr("Spawner: Cannot spawn - MeleeEnemyScene is null!");
            return;
        }

        var enemy = MeleeEnemyScene.Instantiate<MeleeEnemy>();
        if (enemy == null)
        {
            GD.PrintErr("Spawner: Failed to instantiate enemy from scene!");
            return;
        }

        enemy.GlobalPosition = GlobalPosition;
        
        var parent = GetParent();
        if (parent != null)
        {
            parent.AddChild(enemy);
            GD.Print("Spawner: Spawned enemy at ", GlobalPosition);
        }
        else
        {
            GD.PrintErr("Spawner: Cannot spawn - no parent node!");
            enemy.QueueFree();
        }
    }
}
