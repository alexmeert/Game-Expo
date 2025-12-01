using Godot;
using System;

public partial class Perk : Item
{
    [Export] public float HpBonus = 0f;
    [Export] public float DmgBonus = 0f;
    [Export] public float AtkSpdBonus = 0f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float DefBonus = 0f; // Percentage bonus (0.0 to 1.0)
    [Export] public float SpdBonus = 0f;
    [Export] public float Duration = 10f; // Duration in seconds

    private float _remainingTime = 0f;
    private bool _isActive = false;

    public float RemainingTime => _remainingTime;
    public bool IsActive => _isActive;

    public override void Apply(Player player)
    {
        if (player == null)
            return;

        // If already active, just refresh the duration
        if (_isActive)
        {
            _remainingTime = Duration;
            GD.Print($"Refreshed perk: {ItemName} (Duration: {Duration}s)");
            return;
        }

        // Apply temporary stat modifications
        player.MaxHP += HpBonus;
        player.HP += HpBonus; // Also increase current HP
        player.DMG += DmgBonus;
        player.ATKSPD += AtkSpdBonus;
        // DEF is a percentage, so we add the bonus and clamp to 0-1
        player.DEF = MathF.Min(1f, player.DEF + DefBonus);
        player.SPD += SpdBonus;

        _remainingTime = Duration;
        _isActive = true;

        GD.Print($"Applied perk: {ItemName} (Duration: {Duration}s)");
        GD.Print($"  HP: +{HpBonus}, DMG: +{DmgBonus}, ATKSPD: +{AtkSpdBonus}, DEF: +{DefBonus * 100:F1}%, SPD: +{SpdBonus}");
    }

    public override void Remove(Player player)
    {
        if (player == null || !_isActive)
            return;

        // Remove temporary stat modifications
        player.MaxHP -= HpBonus;
        player.DMG -= DmgBonus;
        player.ATKSPD -= AtkSpdBonus;
        // DEF is a percentage, subtract bonus and clamp to 0-1
        player.DEF = MathF.Max(0f, player.DEF - DefBonus);
        player.SPD -= SpdBonus;

        // Ensure HP doesn't go below 0 or above MaxHP
        if (player.HP < 0)
            player.HP = 0;
        if (player.HP > player.MaxHP)
            player.HP = player.MaxHP;

        _isActive = false;
        _remainingTime = 0f;

        GD.Print($"Removed perk: {ItemName}");
    }

    public void Update(float delta)
    {
        if (!_isActive)
            return;

        _remainingTime -= delta;

        if (_remainingTime <= 0f)
        {
            _remainingTime = 0f;
            _isActive = false;
        }
    }
}

