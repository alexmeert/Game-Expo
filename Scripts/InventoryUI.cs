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

		IconListContainer.Alignment = BoxContainer.AlignmentMode.Begin;

		closeArea = GetNodeOrNull<Control>("closeArea");
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
		if (what == NotificationEnterTree)
		{
			string scene = GetTree().CurrentScene?.SceneFilePath;

			if (scene == "res://Scenes/Menus/MainMenu.tscn")
			{
				GlobalInventory.Instance.ClearUpgrades();
				Clear();
			}
		}
	}

	public void AddUpgrade(Upgrade u)
	{
		if (IconListContainer == null || u.Icon == null)
			return;

		// Container for each upgrade entry
		var vbox = new VBoxContainer
		{
			Alignment = BoxContainer.AlignmentMode.Center,
			MouseFilter = Control.MouseFilterEnum.Stop
		};

		// TextureRect directly, pixel-perfect like popup
		var icon = new TextureRect
		{
			Texture = u.Icon,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			CustomMinimumSize = new Vector2(64, 64) // size you want for inventory
		};

		vbox.AddChild(icon);

		// Name label
		var label = new Label
		{
			Text = u.ItemName,
			HorizontalAlignment = HorizontalAlignment.Center
		};
		vbox.AddChild(label);

		// Stats label
		var statsLabel = new Label
		{
			Text = u.GetStatSummary(),
			HorizontalAlignment = HorizontalAlignment.Center,
			Modulate = new Color(0.8f, 0.8f, 0.8f)
		};
		statsLabel.AddThemeFontSizeOverride("font_size", 12);
		vbox.AddChild(statsLabel);

		IconListContainer.AddChild(vbox);

		// Click handler to open popup
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

		currentPopup.Position = mousePos;
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
		foreach (Node child in IconListContainer.GetChildren())
			child.QueueFree();
	}
}
