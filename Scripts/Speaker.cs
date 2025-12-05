using Godot;
using System;

public partial class Speaker : BasicEntity
{
	[Export] private Label HPValueLabel;

	protected override void InitializeEntity()
	{
		base.InitializeEntity();
		// Set initial stats for the speaker
		// Adjust these values as needed for your boss fight
		SetStats(hp: 500, dmg: 0, atkspd: 0, def: 0, spd: 0);
	}

	public override void _Ready()
	{
		base._Ready();
		UpdateHPLabel();
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

	protected override void OnDeath()
	{
		base.OnDeath();
		// Add any speaker-specific death behavior here
		EmitSignal(SignalName.EnemyDied);
	}
}
