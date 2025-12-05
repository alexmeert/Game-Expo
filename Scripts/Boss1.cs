using Godot;
using System;

public partial class Boss : BasicEntity
{
    private int phase = 1;

    [Export] private AnimatedSprite2D Sprite;
    [Export] private double Phase2Threshold = 0.6;
    [Export] private double Phase3Threshold = 0.2;

    protected override void InitializeEntity()
    {
        base.InitializeEntity();
        SetStats(hp: 1500, dmg: 0, atkspd: 0, def: 0, spd: 0);
    }

    public override void _Ready()
    {
        base._Ready();
        Sprite?.Play("Phase1");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsAlive) return;

        if (HPPercent < Phase3Threshold && phase != 3)
            SetPhase(3);
        else if (HPPercent < Phase2Threshold && phase == 1)
            SetPhase(2);
    }

    private void SetPhase(int newPhase)
    {
        phase = newPhase;
        switch (phase)
        {
            case 1: Sprite?.Play("Phase1"); break;
            case 2: Sprite?.Play("Phase2"); break;
            case 3: Sprite?.Play("Phase3"); break;
        }
    }

    protected override void OnDeath()
    {
        EmitSignal(SignalName.EnemyDied);
    }
}
