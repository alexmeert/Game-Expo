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

		// Invisible square closes popup
		_closeArea.GuiInput += (InputEvent @event) =>
		{
			if (@event is InputEventMouseButton mb &&
				mb.Pressed &&
				mb.ButtonIndex == MouseButton.Left)
			{
				Hide();
			}
		};

		// Configure icon to prevent stretching, scale, and center
		if (IconRect != null)
		{
			IconRect.StretchMode = TextureRect.StretchModeEnum.Keep;
			IconRect.Scale = new Vector2(1.5f, 1.5f); // Scale up by 1.5x
		}

		// Start hidden
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
