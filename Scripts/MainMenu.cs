using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GetNode<Button>("CenterContainer/VBoxContainer/PlayButton").Pressed += OnStartPressed;
		GetNode<Button>("CenterContainer/VBoxContainer/ItemsButton").Pressed += OnItemsPressed;
		GetNode<Button>("CenterContainer/VBoxContainer/ControlsButton").Pressed += OnControlsPressed;
		GetNode<Button>("CenterContainer/VBoxContainer/QuitButton").Pressed += OnQuitPressed;
	}


	private void OnStartPressed()
	{
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

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}
}
