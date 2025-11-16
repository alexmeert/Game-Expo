using Godot;
using System;
using System.Collections.Generic;

public partial class BasicEntity : CharacterBody2D
{
    public Dictionary<string, float> Stats = new Dictionary<string, float>()
    {
        { "HP", 100 },
        { "DMG", 10 },
        { "ATKSPD", 1.0f },
        { "DEF", 0 },
        { "SPD", 100 },
    };

    public float HP
    {
        get => Stats["HP"];
        set => Stats["HP"] = value;
    }

    public float DMG
    {
        get => Stats["DMG"];
        set => Stats["DMG"] = value;
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

    public void SetStats(float hp, float dmg, float atkspd, float def, float spd)
    {
        HP = hp;
        DMG = dmg;
        ATKSPD = atkspd;
        DEF = def;
        SPD = spd;
    }

    public virtual void TakeDamage(float dmg)
    {
        float finalDamage = MathF.Max(1, dmg - DEF);
        HP -= finalDamage;

        if (HP <= 0)
            Die();
    }

    protected virtual void Die()
    {
        QueueFree();
    }
}
