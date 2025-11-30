using Godot;
using System;

public partial class Upgrade : Item
{
    [Export] public float HpBonus = 0f;
    [Export] public float DmgBonus = 0f;
    [Export] public float AtkSpdBonus = 0f;
    [Export] public float DefBonus = 0f;
    [Export] public float SpdBonus = 0f;

    public override void Apply(Player player)
    {
        if (player == null)
            return;

        // Permanently modify player stats
        player.MaxHP += HpBonus;
        player.HP += HpBonus; // Also increase current HP
        player.DMG += DmgBonus;
        player.ATKSPD += AtkSpdBonus;
        player.DEF += DefBonus;
        player.SPD += SpdBonus;

        GD.Print($"Applied upgrade: {ItemName}");
        GD.Print($"  HP: +{HpBonus}, DMG: +{DmgBonus}, ATKSPD: +{AtkSpdBonus}, DEF: +{DefBonus}, SPD: +{SpdBonus}");
    }

    public override void Remove(Player player)
    {
        if (player == null)
            return;

        // Remove permanent stat modifications
        player.MaxHP -= HpBonus;
        player.DMG -= DmgBonus;
        player.ATKSPD -= AtkSpdBonus;
        player.DEF -= DefBonus;
        player.SPD -= SpdBonus;

        // Ensure HP doesn't go below 0
        if (player.HP < 0)
            player.HP = 0;
    }
}

