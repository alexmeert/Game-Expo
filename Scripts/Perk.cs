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

	if (_isActive)
	{
		_remainingTime = Duration;
		GD.Print($"Refreshed perk: {ItemName} (Duration: {Duration}s)");
		return;
	}

	player.MaxHP += HpBonus;
	player.HP += HpBonus; 
	player.DMG += DmgBonus;
	player.ATKSPD += AtkSpdBonus;
	player.DEF = MathF.Min(1f, player.DEF + DefBonus);
	player.SPD += SpdBonus;

	_remainingTime = Duration;
	_isActive = true;

	GD.Print($"Applied perk: {ItemName} (Duration: {Duration}s)");
}

public override void Remove(Player player)
{
	if (player == null || !_isActive)
		return;

	player.MaxHP -= HpBonus;
	player.DMG -= DmgBonus;
	player.ATKSPD -= AtkSpdBonus;
	player.DEF = MathF.Max(0f, player.DEF - DefBonus);
	player.SPD -= SpdBonus;

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
