using Godot;
using System;

public partial class InventoryUI : Control
{
	[Export] public HBoxContainer IconListContainer;
	[Export] public PackedScene UpgradePopupScene;

	private UpgradePopup currentPopup;
	private Control closeArea;

	public static InventoryUI Instance;

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		Visible = false;

		if (IconListContainer == null)
			GD.PrintErr("InventoryUI.IconListContainer is NOT SET!");

		IconListContainer.Alignment = BoxContainer.AlignmentMode.Begin;

		closeArea = GetNodeOrNull<Control>("CloseArea");
		if (closeArea != null)
		{
			closeArea.GuiInput += (InputEvent ev) =>
			{
				if (ev is InputEventMouseButton mb &&
					mb.Pressed &&
					mb.ButtonIndex == MouseButton.Left)
				{
					Toggle();
				}
			};
		}
	}

	
	public override void _Notification(int what)
	{
		// Node has been added to the tree
		if (what == NotificationEnterTree)
		{
			// Check if this is MainMenu
			string scene = GetTree().CurrentScene?.SceneFilePath;

			if (scene == "res://Scenes/Menus/MainMenu.tscn")
			{
				GD.Print("InventoryUI: Entered MainMenu → clearing inventory");

				GlobalInventory.Instance.ClearUpgrades();
				Clear();
			}
		}
	}

	public void AddUpgrade(Upgrade u)
	{
		if (IconListContainer == null || u.Icon == null)
			return;

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

		vbox.GuiInput += (InputEvent ev) =>
		{
			if (ev is InputEventMouseButton mb &&
				mb.Pressed &&
				mb.ButtonIndex == MouseButton.Left)
			{
				OpenPopup(u);
			}
		};
	}

	private void OpenPopup(Upgrade u)
	{
		if (currentPopup == null)
		{
			currentPopup = UpgradePopupScene.Instantiate<UpgradePopup>();
			AddChild(currentPopup);
		}

		currentPopup.SetUpgrade(u);
		currentPopup.Show();

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

		currentPopup.Position = new Vector2I((int)mousePos.X, (int)mousePos.Y);
	}

	public void Toggle()
	{
		Visible = !Visible;

		if (Visible)
		{
			Clear();

			foreach (var upgrade in GlobalInventory.Instance.GetUpgrades())
				AddUpgrade(upgrade);
		}

		Engine.TimeScale = Visible ? 0f : 1f;
	}

	public void Clear()
	{
		if (IconListContainer == null) return;

		foreach (Node child in IconListContainer.GetChildren())
			child.QueueFree();
	}
}
