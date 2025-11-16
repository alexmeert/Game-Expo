using Godot;
using System;
using System.Collections.Generic;

public partial class BasicEntity : CharacterBody2D
{
    public Dictionary<string, float> Stats = new Dictionary<string, float>()
    {
        { "HP", 100 },
        { "ATK", 10 },
        { "ATKSPD", 1.0f },
        { "DEF", 0 },
        { "SPD", 100 },
    };

    public float HP
    {
        get => Stats["HP"];
        set => Stats["HP"] = value;
    }

    public float ATK
    {
        get => Stats["ATK"];
        set => Stats["ATK"] = value;
    }

    public float ATKSPD
    {
        get => Stats["ATKSPD"];
        set => Stats["ATKSPD"] = value;
    }

    public float DEF
    {
        get => Stats["DEF"];
        set => Stats["DEF"] = value;
    }

    public float SPD
    {
        get => Stats["SPD"];
        set => Stats["SPD"] = value;
    }

    public virtual void TakeDamage(float amount)
    {
        float finalDamage = MathF.Max(1, amount - DEF);
        HP -= finalDamage;

        if (HP <= 0)
            Die();
    }

    protected virtual void Die()
    {
        QueueFree();
    }
}
