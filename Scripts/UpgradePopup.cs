using Godot;
using System;

public partial class UpgradePopup : Control
{
	[Export] public TextureRect IconRect;
	[Export] public Label NameLabel;
	[Export] public Label RarityLabel;
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

		// Start hidden
		Hide();
	}

	public void SetUpgrade(Upgrade u)
	{
		IconRect.Texture = u.Icon;
		NameLabel.Text = u.ItemName;
		RarityLabel.Text = u.Rarity.ToString();
		DescriptionLabel.Text = u.Description;
	}
}
