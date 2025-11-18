using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GetNode<Button>("VBoxContainer/StartButton").Pressed += OnStartPressed;
		GetNode<Button>("VBoxContainer/QuitButton").Pressed += OnQuitPressed;
	}

	private void OnStartPressed()
	{
		
		RandomSceneLoader.Instance.LoadNextRoom();
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}
}
