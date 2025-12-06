using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		// Reset inventory automatically when MainMenu loads
		GlobalInventory.Instance.ClearUpgrades();
		
		RandomSceneLoader.Instance?.Reset();

		GetNode<Button>("VBoxContainer/PlayButton").Pressed += OnStartPressed;
		GetNode<Button>("VBoxContainer/ItemsButton").Pressed += OnItemsPressed;
		GetNode<Button>("VBoxContainer/ControlsButton").Pressed += OnControlsPressed;
		GetNode<Button>("VBoxContainer/SettingsButton").Pressed += OnSettingsPressed;
		GetNode<Button>("VBoxContainer/QuitButton").Pressed += OnQuitPressed;
	}

	private async void OnStartPressed()
	{
		if (ScreenFader.Instance != null)
		{
			await ScreenFader.Instance.FadeOut(); // fade to black
		}

		// Load next scene AFTER fade
		RandomSceneLoader.Instance.LoadNextRoom();
	}



	private void OnItemsPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/Menus/ItemsMenu.tscn");
	}

	private void OnControlsPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/Menus/ControlsMenu.tscn");
	}

	private void OnSettingsPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/Menus/SettingsMenu.tscn");
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}
}
