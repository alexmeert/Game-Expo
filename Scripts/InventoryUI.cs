using Godot;
using System;

public partial class InventoryUI : Control
{
	[Export] public HBoxContainer IconListContainer;
	[Export] public PackedScene UpgradePopupScene;

	private UpgradePopup currentPopup; // Reusable popup
	private Control closeArea;         // Assigned from scene

	public static InventoryUI Instance;

	public override void _EnterTree() => Instance = this;

	public override void _Ready()
	{
		Visible = false;

		if (IconListContainer == null)
			GD.PrintErr("InventoryUI.IconListContainer is NOT SET!");

		IconListContainer.Alignment = BoxContainer.AlignmentMode.Begin;

		// Look for a Control node named "CloseArea" in the scene
		closeArea = GetNodeOrNull<Control>("CloseArea");
		if (closeArea != null)
		{
			closeArea.GuiInput += (InputEvent @event) =>
			{
				if (@event is InputEventMouseButton mb &&
					mb.Pressed &&
					mb.ButtonIndex == MouseButton.Left)
				{
					Toggle(); // hide inventory
				}
			};
		}
	}

	public void AddUpgrade(Upgrade u)
	{
		if (IconListContainer == null || u.Icon == null)
			return;

		// Create icon + name container
		var vbox = new VBoxContainer
		{
			Alignment = BoxContainer.AlignmentMode.Center,
			CustomMinimumSize = new Vector2(64, 80)
		};

		var icon = new TextureRect
		{
			Texture = u.Icon,
			CustomMinimumSize = new Vector2(64, 64),
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
		};
		vbox.AddChild(icon);

		var label = new Label
		{
			Text = u.ItemName,
			HorizontalAlignment = HorizontalAlignment.Center
		};
		vbox.AddChild(label);

		IconListContainer.AddChild(vbox);

		// CLICK TO OPEN POPUP
		vbox.GuiInput += (InputEvent @event) =>
		{
			if (@event is InputEventMouseButton mouseEvent &&
				mouseEvent.Pressed &&
				mouseEvent.ButtonIndex == MouseButton.Left)
			{
				OpenPopup(u);
			}
		};
	}

	private void OpenPopup(Upgrade u)
	{
		// Create popup once
		if (currentPopup == null)
		{
			currentPopup = UpgradePopupScene.Instantiate<UpgradePopup>();
			AddChild(currentPopup);
		}

		// Update content
		currentPopup.SetUpgrade(u);

		// Show it
		currentPopup.Show();

		// Position it
		UpdatePopupPosition();
	}

	private void UpdatePopupPosition()
	{
		if (currentPopup == null) return;

		Vector2 mousePos = GetGlobalMousePosition() + new Vector2(10, 10);
		Vector2 screenSize = GetViewportRect().Size;
		Vector2 popupSize = currentPopup.Size;

		if (mousePos.X + popupSize.X > screenSize.X)
			mousePos.X = screenSize.X - popupSize.X;

		if (mousePos.Y + popupSize.Y > screenSize.Y)
			mousePos.Y = screenSize.Y - popupSize.Y;

		currentPopup.Position = new Vector2I(
			(int)mousePos.X,
			(int)mousePos.Y
		);
	}

	public void Toggle()
	{
		Visible = !Visible;
		Engine.TimeScale = Visible ? 0f : 1f;
	}

	public void Clear()
	{
		if (IconListContainer == null) return;

		foreach (Node child in IconListContainer.GetChildren())
			child.QueueFree();
	}
}
