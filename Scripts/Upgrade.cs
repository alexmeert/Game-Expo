using Godot;
using System;

public enum UpgradeRarity
{
    Common,    // 5% increase
    Rare,      // 10% increase
    Epic,      // 20% increase
    Legendary  // 35% increase
}

public partial class Upgrade : Item
{
    [Export] public UpgradeRarity Rarity = UpgradeRarity.Common;
    
    // Flags to indicate which stats this upgrade affects (1.0 = affects this stat, 0.0 = doesn't affect)
    // The actual percentage increase is determined by the rarity
    [Export(PropertyHint.Range, "0,1,0.01")] public float HpPercent = 0f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float DmgPercent = 0f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float AtkSpdPercent = 0f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float DefPercent = 0f; // Percentage bonus (0.0 to 1.0)
    [Export(PropertyHint.Range, "0,1,0.01")] public float SpdPercent = 0f;


    public override void _Ready()
    {
        base._Ready();
        var area = GetNode<Area2D>("PickupArea");
        area.BodyEntered += OnPickup;
    }

    private void OnPickup(Node body)
    {
        if (body is Player player)
        {
            player.ApplyUpgrade(this); // applies stats
            GD.Print($"{Name} collected!");
            QueueFree(); // removes the upgrade scene from the map
        }
    }


    // Get the rarity multiplier
    private float GetRarityMultiplier()
    {
        return Rarity switch
        {
            UpgradeRarity.Common => 0.05f,    // 5%
            UpgradeRarity.Rare => 0.10f,      // 10%
            UpgradeRarity.Epic => 0.20f,      // 20%
            UpgradeRarity.Legendary => 0.35f, // 35%
            _ => 0.05f
        };
    }

    // Store the actual stat increases for removal
    private float _appliedHpIncrease = 0f;
    private float _appliedDmgIncrease = 0f;
    private float _appliedAtkSpdIncrease = 0f;
    private float _appliedDefIncrease = 0f;
    private float _appliedSpdIncrease = 0f;

    public override void Apply(Player player)
    {
        if (player == null)
            return;

        float rarityMultiplier = GetRarityMultiplier();

        // Calculate increases based on base stats
        // If a stat field is > 0, apply the rarity percentage to that stat
        // Example: Common (5%) HP upgrade on 100 base HP = 100 * 0.05 = 5 HP increase
        _appliedHpIncrease = HpPercent > 0 ? player.GetBaseMaxHP() * rarityMultiplier * HpPercent : 0f;
        _appliedDmgIncrease = DmgPercent > 0 ? player.GetBaseDMG() * rarityMultiplier * DmgPercent : 0f;
        _appliedAtkSpdIncrease = AtkSpdPercent > 0 ? player.GetBaseATKSPD() * rarityMultiplier * AtkSpdPercent : 0f;
        _appliedDefIncrease = DefPercent > 0 ? rarityMultiplier * DefPercent : 0f; // DEF is already a percentage
        _appliedSpdIncrease = SpdPercent > 0 ? player.GetBaseSPD() * rarityMultiplier * SpdPercent : 0f;

        // Apply the calculated increases
        player.MaxHP += _appliedHpIncrease;
        player.HP += _appliedHpIncrease; // Also increase current HP
        player.DMG += _appliedDmgIncrease;
        player.ATKSPD += _appliedAtkSpdIncrease;
        // DEF is a percentage, so we add the bonus and clamp to 0-1
        player.DEF = MathF.Min(1f, player.DEF + _appliedDefIncrease);
        player.SPD += _appliedSpdIncrease;

        string rarityName = Rarity.ToString();
        float rarityPercent = rarityMultiplier * 100f;
        
        GD.Print($"Applied {rarityName} upgrade: {ItemName} ({rarityPercent:F0}% increase)");
        if (HpPercent > 0) GD.Print($"  HP: +{_appliedHpIncrease:F1} ({rarityPercent * HpPercent:F1}% of base)");
        if (DmgPercent > 0) GD.Print($"  DMG: +{_appliedDmgIncrease:F1} ({rarityPercent * DmgPercent:F1}% of base)");
        if (AtkSpdPercent > 0) GD.Print($"  ATKSPD: +{_appliedAtkSpdIncrease:F2} ({rarityPercent * AtkSpdPercent:F1}% of base)");
        if (DefPercent > 0) GD.Print($"  DEF: +{_appliedDefIncrease * 100:F1}% ({rarityPercent * DefPercent:F1}% of base)");
        if (SpdPercent > 0) GD.Print($"  SPD: +{_appliedSpdIncrease:F1} ({rarityPercent * SpdPercent:F1}% of base)");
    }

    public override void Remove(Player player)
    {
        if (player == null)
            return;

        // Remove the exact increases that were applied
        player.MaxHP -= _appliedHpIncrease;
        player.DMG -= _appliedDmgIncrease;
        player.ATKSPD -= _appliedAtkSpdIncrease;
        // DEF is a percentage, subtract bonus and clamp to 0-1
        player.DEF = MathF.Max(0f, player.DEF - _appliedDefIncrease);
        player.SPD -= _appliedSpdIncrease;

        // Ensure HP doesn't go below 0
        if (player.HP < 0)
            player.HP = 0;

        // Reset applied increases
        _appliedHpIncrease = 0f;
        _appliedDmgIncrease = 0f;
        _appliedAtkSpdIncrease = 0f;
        _appliedDefIncrease = 0f;
        _appliedSpdIncrease = 0f;
    }
}

