using Godot;
using System;

public partial class BasicEntity : CharacterBody2D
{
    private float _hp;
    private float _maxHp;
    private float _dmg;
    private float _atkspd;
    private float _def;
    private float _spd;

    public float HP
    {
        get => _hp;
        set
        {
            _hp = MathF.Max(0, MathF.Min(value, MaxHP));
            OnHPChanged();
        }
    }

    public float MaxHP
    {
        get => _maxHp;
        set
        {
            _maxHp = MathF.Max(1, value);
            if (_hp > _maxHp)
                _hp = _maxHp;
        }
    }

    public float DMG
    {
        get => _dmg;
        set => _dmg = MathF.Max(0, value);
    }

    public float ATKSPD
    {
        get => _atkspd;
        set => _atkspd = MathF.Max(0, value);
    }

    public float DEF
    {
        get => _def;
        set => _def = MathF.Max(0, MathF.Min(1, value)); // Clamp between 0 and 1 (0% to 100% damage reduction)
    }

    public float SPD
    {
        get => _spd;
        set => _spd = MathF.Max(0, value);
    }

    public bool IsAlive => HP > 0;
    public float HPPercent => MaxHP > 0 ? HP / MaxHP : 0f;

    public override void _Ready()
    {
        base._Ready();
        InitializeEntity();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsAlive)
            return;

        HandleMovement(delta);
        MoveAndSlide();
    }

    protected virtual void InitializeEntity()
    {
        SetStats(hp: 100, dmg: 10, atkspd: 1.0f, def: 0f, spd: 100);
        OnEntityInitialized();
    }

    public void SetStats(float hp, float dmg, float atkspd, float def, float spd)
    {
        MaxHP = hp;
        HP = hp;
        DMG = dmg;
        ATKSPD = atkspd;
        DEF = def;
        SPD = spd;
    }

    protected virtual void HandleMovement(double delta)
    {
    }

    public virtual void TakeDamage(float dmg)
    {
        if (!IsAlive)
            return;

        // DEF is a percentage (0-1) that determines damage reduction
        // If DEF is 0.5 (50%), you take 50% of the damage (block 50%)
        float damageMultiplier = 1f - DEF;
        float finalDamage = dmg * damageMultiplier;
        
        // Ensure at least 1 damage is taken (unless DEF is 100%)
        if (DEF < 1f)
        {
            finalDamage = MathF.Max(1, finalDamage);
        }
        
        HP -= finalDamage;

        OnTakeDamage(finalDamage);

        if (HP <= 0)
        {
            Die();
        }
    }

    public virtual void Heal(float amount)
    {
        if (!IsAlive)
            return;

        float oldHP = HP;
        HP += amount;
        float actualHeal = HP - oldHP;

        if (actualHeal > 0)
            OnHeal(actualHeal);
    }

    protected virtual void Die()
    {
        OnDeath();
        QueueFree();
    }

    protected virtual void OnEntityInitialized() { }

    protected virtual void OnHPChanged() { }

    protected virtual void OnTakeDamage(float damage) { }

    protected virtual void OnHeal(float amount) { }

    protected virtual void OnDeath() { }
}
