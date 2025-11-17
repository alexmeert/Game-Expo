using Godot;
using System;

public partial class MeleeEnemy : BasicEntity
{
    protected Node2D TargetPlayer { get; private set; }

    protected override void InitializeEntity()
    {
        base.InitializeEntity();
        SetStats(hp: 50, dmg: 1, atkspd: 1.2f, def: 1, spd: 20);
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

        Vector2 direction = (TargetPlayer.GlobalPosition - GlobalPosition).Normalized();
        Velocity = direction * SPD;
    }

    protected virtual void FindPlayer()
    {
        var scene = GetTree().CurrentScene;
        if (scene != null)
        {
            TargetPlayer = scene.GetNodeOrNull<Node2D>("MainCharacter");
        }
    }
}
