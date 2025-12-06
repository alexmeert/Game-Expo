using Godot;
using System;

public partial class Speaker : BasicEntity
{
    [Export] private Label HPValueLabel;
    [Export] private SpeakerGun SpeakerGun;
    [Export] private AnimatedSprite2D Sprite;

    protected override void InitializeEntity()
    {
        base.InitializeEntity();
        SetStats(hp: 500, dmg: 20, atkspd: 0, def: 0, spd: 0);
    }

    public override void _Ready()
    {
        base._Ready();
        UpdateHPLabel();

        // Play Default animation
        if (Sprite == null)
            Sprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");

        if (Sprite != null)
            Sprite.Play("Default");
        else
            GD.PrintErr("Speaker: No AnimatedSprite2D Sprite found!");

        // Setup gun
        if (SpeakerGun == null)
            SpeakerGun = GetNodeOrNull<SpeakerGun>("SpeakerGun");

        if (SpeakerGun != null)
        {
            SpeakerGun.Owner = this;

            if (SpeakerGun.SpeakerProjectileManager == null)
            {
                var projectileManager = SpeakerGun.GetNodeOrNull<SpeakerProjectileManager>("SpeakerProjectileManager");
                if (projectileManager != null)
                {
                    SpeakerGun.SpeakerProjectileManager = projectileManager;
                    projectileManager.Owner = this;
                }
                else
                {
                    GD.PrintErr("Speaker: Could not find SpeakerProjectileManager as child of SpeakerGun!");
                }
            }
            else
            {
                SpeakerGun.SpeakerProjectileManager.Owner = this;
            }
        }
        else
        {
            GD.PrintErr("Speaker: SpeakerGun is not assigned or found!");
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
            // Stop all speaker activities
            SetPhysicsProcess(false);
            if (SpeakerGun != null)
            {
                SpeakerGun.SetProcess(false);
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
