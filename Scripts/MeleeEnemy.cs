using Godot;
using System;

public partial class MeleeEnemy : BasicEntity
{
    private Node2D _player;

public override void _Ready()
{
    SetStats(hp: 50, dmg: 1, atkspd: 1.2f, def: 1, spd: 20);

    _player = GetTree().CurrentScene.GetNode<Node2D>("MainCharacter");
}


    public override void _PhysicsProcess(double delta)
    {
        if (_player == null)
            return;

        float speed = SPD;

        Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();

        Velocity = direction * speed;
        MoveAndSlide();
    }
}
