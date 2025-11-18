using Godot;
using System;

public partial class MeleeEnemy : BasicEntity
{
    [Export] private int Hp = 50;
    [Export] private int Dmg = 1;
    [Export] private float AtkSpd = 1.2f;
    [Export] private int Def = 1;
    [Export] private float Spd = 20;

    protected Node2D TargetPlayer { get; private set; }

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
