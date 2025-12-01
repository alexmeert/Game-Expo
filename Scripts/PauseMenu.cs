using Godot;
using System;

public partial class PauseMenu : Control
{
	private const string MainMenuScenePath = "res://Scenes/Menus/MainMenu.tscn";

	public override void _Ready()
	{
		// Start hidden
		Hide();

		// Ensure the pause menu still processes input while the game is paused
		ProcessMode = ProcessModeEnum.Always;

		// Hook up buttons (adjust the node paths if your scene hierarchy is different)
		GetNode<Button>("VBoxContainer/ResumeButton").Pressed += OnResumePressed;
		GetNode<Button>("VBoxContainer/ControlsButton").Pressed += OnControlsPressed;
		GetNode<Button>("VBoxContainer/QuitToMenuButton").Pressed += OnQuitToMenuPressed;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		// Toggle pause with Esc / ui_cancel
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
		// Open a controls scene or overlay.
		// Replace this with your actual controls scene path or logic.
		GetTree().ChangeSceneToFile("res://Scenes/Menus/ControlsMenu.tscn");
	}

	private void OnQuitToMenuPressed()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile(MainMenuScenePath);
	}
}
