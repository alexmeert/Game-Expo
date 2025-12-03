using Godot;
using System;

public partial class PauseMenu : Control
{

	public override void _Ready()
	{
		Hide();
		ProcessMode = ProcessModeEnum.Always;

		// Hook up buttons (adjust the node paths if your scene hierarchy is different)
		GetNode<Button>("CenterContainer/VBoxContainer/ResumeButton").Pressed += OnResumePressed;
		GetNode<Button>("CenterContainer/VBoxContainer/ControlsButton").Pressed += OnControlsPressed;
		GetNode<Button>("CenterContainer/VBoxContainer/QuitButton").Pressed += OnQuitToMenuPressed;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			if (Visible)
				ResumeGame();
			else
				PauseGame();
		}
	}

	private void PauseGame()
	{
		GetTree().Paused = true;
		Show();
	}

	private void ResumeGame()
	{
		GetTree().Paused = false;
		Hide();
	}

	private void OnResumePressed()
	{
		ResumeGame();
	}

	private void OnControlsPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/Menus/ControlsMenu.tscn");
	}

	private void OnQuitToMenuPressed()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://Scenes/Menus/MainMenu.tscn");
	}
}
