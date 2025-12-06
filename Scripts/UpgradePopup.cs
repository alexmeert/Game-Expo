using Godot;
using System;

public partial class UpgradePopup : Control
{
	[Export] public TextureRect IconRect;
	[Export] public Label NameLabel;
	[Export] public Label EffectLabel;
	[Export] public Label DescriptionLabel;

	private Control _closeArea;

	public override void _Ready()
	{
		_closeArea = GetNode<Control>("CloseArea");

		// Close popup when clicking invisible area
		_closeArea.GuiInput += (InputEvent ev) =>
		{
			if (ev is InputEventMouseButton mb &&
				mb.Pressed &&
				mb.ButtonIndex == MouseButton.Left)
			{
				Hide();
			}
		};

		// Proper icon sizing – no scaling!
		if (IconRect != null)
		{
			IconRect.StretchMode = TextureRect.StretchModeEnum.KeepCentered;
			IconRect.CustomMinimumSize = new Vector2(128, 128); // Bigger inside popup
		}

		Hide();
	}

	public void SetUpgrade(Upgrade u)
	{
		IconRect.Texture = u.Icon;
		NameLabel.Text = u.ItemName;
		EffectLabel.Text = u.Effect;
		DescriptionLabel.Text = u.Description;
	}
}
