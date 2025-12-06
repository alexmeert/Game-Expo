using Godot;
using System;

public partial class Perk : Item
{
[Export] public float HpBonus = 0f;
[Export] public float DmgBonus = 0f;
[Export(PropertyHint.Range, "0,1,0.01")] public float DefBonus = 0f; // Percentage bonus (0.0 to 1.0)
[Export] public float AtkSpdBonus = 0f;
[Export] public float SpdBonus = 0f;
[Export] public float Duration = 10f; // Duration in seconds


private float _remainingTime = 0f;
private bool _isActive = false;
private bool _statsApplied = false; // Track if stats have been applied to prevent duplicates

public float RemainingTime => _remainingTime;
public bool IsActive => _isActive;

public override void _Ready()
{
	base._Ready();

	
	var area = GetNode<Area2D>("Area2D");
	area.Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
}

private void OnBodyEntered(Node body)
{
	if (body is Player player)
	{
		// Use player's ApplyPerk method to ensure it's added to the active perks list
		player.ApplyPerk(this);

		//Remove perk from scene after pickup
		QueueFree();
	}
}

public override void Apply(Player player)
{
	if (player == null)
		return;

	// If already active, just refresh the timer
	if (_isActive && _statsApplied)
	{
		_remainingTime = Duration;
		GD.Print($"Refreshed perk: {ItemName} (Duration: {Duration}s)");
		return;
	}

	// Apply stats only if not already applied
	if (!_statsApplied)
	{
		player.MaxHP += HpBonus;
		// Add HP bonus, but ensure it doesn't exceed MaxHP
		float newHP = player.HP + HpBonus;
		player.HP = MathF.Min(newHP, player.MaxHP);
		
		player.DMG += DmgBonus;
		player.ATKSPD += AtkSpdBonus;
		player.DEF = MathF.Min(1f, player.DEF + DefBonus);
		player.SPD += SpdBonus;
		
		_statsApplied = true;
		GD.Print($"Applied perk: {ItemName} (Duration: {Duration}s)");
	}

	_remainingTime = Duration;
	_isActive = true;
}

public override void Remove(Player player)
{
	if (player == null || !_isActive || !_statsApplied)
		return;

	// Remove stats
	player.MaxHP -= HpBonus;
	player.DMG -= DmgBonus;
	player.ATKSPD -= AtkSpdBonus;
	player.DEF = MathF.Max(0f, player.DEF - DefBonus);
	player.SPD -= SpdBonus;
	
	// Ensure HP doesn't exceed MaxHP after removal
	if (player.HP > player.MaxHP)
		player.HP = player.MaxHP;
	
	// Ensure HP doesn't go below 0
	if (player.HP < 0)
		player.HP = 0;

	_isActive = false;
	_statsApplied = false;
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
		// Note: Remove() will be called by Player.UpdatePerks() when IsActive becomes false
	}
}


}
