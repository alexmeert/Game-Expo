using Godot;
using System;

public partial class Boss : BasicEntity
{
    private int phase = 1;
    private bool canTakeDamage = false;


    [Export] private AnimatedSprite2D Sprite;
    [Export] private double Phase2Threshold = 0.6;
    [Export] private double Phase3Threshold = 0.2;
    [Export] private Label HPValueLabel;
    [Export] private BossGun BossGun;
    [Export] private CharacterBody2D LeftSpeaker;
    [Export] private CharacterBody2D RightSpeaker;


    protected override void InitializeEntity()
    {
        base.InitializeEntity();
        SetStats(hp: 1500, dmg: 30, atkspd: 0, def: 0, spd: 0);
    }

    public override void _Ready()
    {
        base._Ready();
        Sprite.Play("Phase1");
        UpdateHPLabel();

        // Setup boss gun
        if (BossGun == null)
        {
            BossGun = GetNodeOrNull<BossGun>("BossGun");
        }

        if (BossGun != null && BossProjectileManager != null)
        {
            BossGun.BossProjectileManager = BossProjectileManager;
            BossProjectileManager.Owner = this;
        }

        // Find speakers if not assigned
        if (LeftSpeaker == null || RightSpeaker == null)
        {
            FindSpeakers();
        }

        // Connect to speaker death signals
        ConnectSpeakerSignals();

        // Boss cannot take damage until speakers are dead
        canTakeDamage = false;
    }

    private BossProjectileManager BossProjectileManager
    {
        get
        {
            if (BossGun != null && BossGun.BossProjectileManager != null)
                return BossGun.BossProjectileManager;
            return GetNodeOrNull<BossProjectileManager>("BossProjectileManager");
        }
    }

    private void FindSpeakers()
    {
        var scene = GetTree().CurrentScene;
        if (scene == null) return;

        // Try to find speakers by name or type
        LeftSpeaker = scene.GetNodeOrNull<CharacterBody2D>("LeftSpeaker");
        RightSpeaker = scene.GetNodeOrNull<CharacterBody2D>("RightSpeaker");

        if (LeftSpeaker == null || RightSpeaker == null)
        {
            // Search through children for Speaker nodes
            foreach (Node child in scene.GetChildren())
            {
                if (child is Speaker speaker)
                {
                    if (LeftSpeaker == null)
                    {
                        LeftSpeaker = speaker;
                    }
                    else if (RightSpeaker == null)
                    {
                        RightSpeaker = speaker;
                        break; // Found both
                    }
                }
            }
        }
    }

    private void ConnectSpeakerSignals()
    {
        if (LeftSpeaker is Speaker leftSpeaker)
        {
            if (!leftSpeaker.IsConnected(SignalName.EnemyDied, new Callable(this, nameof(OnSpeakerDied))))
            {
                leftSpeaker.EnemyDied += OnSpeakerDied;
            }
        }

        if (RightSpeaker is Speaker rightSpeaker)
        {
            if (!rightSpeaker.IsConnected(SignalName.EnemyDied, new Callable(this, nameof(OnSpeakerDied))))
            {
                rightSpeaker.EnemyDied += OnSpeakerDied;
            }
        }
    }

    private void OnSpeakerDied()
    {
        // Check if both speakers are dead
        bool leftDead = LeftSpeaker == null || (LeftSpeaker is BasicEntity left && !left.IsAlive);
        bool rightDead = RightSpeaker == null || (RightSpeaker is BasicEntity right && !right.IsAlive);

        if (leftDead && rightDead)
        {
            canTakeDamage = true;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsAlive) return;

        // Check if both speakers are dead
        bool leftDead = LeftSpeaker == null || (LeftSpeaker is BasicEntity left && !left.IsAlive);
        bool rightDead = RightSpeaker == null || (RightSpeaker is BasicEntity right && !right.IsAlive);

        if (leftDead && rightDead && !canTakeDamage)
        {
            canTakeDamage = true;
        }

        // Try to attack if speakers are dead
        if (canTakeDamage && BossGun != null)
        {
            BossGun.TryFire();
        }

        // Update phase based on HP
        if (HPPercent < Phase3Threshold && phase != 3)
            SetPhase(3);
        else if (HPPercent < Phase2Threshold && phase == 1)
            SetPhase(2);
    }

    public override void TakeDamage(float dmg)
    {
        // Boss cannot take damage until both speakers are defeated
        if (!canTakeDamage)
        {
            return;
        }

        base.TakeDamage(dmg);
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

        // Update boss gun phase
        if (BossGun != null)
        {
            BossGun.SetPhase(phase);
        }
    }

    protected override void OnHPChanged()
    {
        base.OnHPChanged();
        UpdateHPLabel();
    }

    protected override void OnMaxHPChanged()
    {
        base.OnMaxHPChanged();
        UpdateHPLabel();
    }

    private void UpdateHPLabel()
    {
        if (HPValueLabel != null)
            HPValueLabel.Text = $"{Mathf.CeilToInt(HP)} / {Mathf.CeilToInt(MaxHP)}";
    }

    protected override void Die()
    {
        EmitSignal(SignalName.EnemyDied);

        // Play death animation
        if (Sprite != null)
        {
            // Stop all boss activities
            SetPhysicsProcess(false);
            if (BossGun != null)
            {
                BossGun.SetProcess(false);
            }
            
            // Play death animation
            Sprite.Play("Death");
            
            // Wait for animation to finish before removing entity
            if (!Sprite.IsConnected(AnimatedSprite2D.SignalName.AnimationFinished, new Callable(this, nameof(OnDeathAnimationFinished))))
            {
                Sprite.AnimationFinished += OnDeathAnimationFinished;
            }
        }
        else
        {
            // No sprite, remove immediately
            base.Die();
        }
    }

    private void OnDeathAnimationFinished()
    {
        if (Sprite != null && Sprite.Animation == "Death")
        {
            // Animation finished, now remove the entity
            base.Die();
        }
    }

    protected override void OnDeath()
    {
        // This is called by base.Die(), but we override Die() to handle animation
    }
}
