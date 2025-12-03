using Godot;
using System;

public partial class PauseMenu : Control
{

	public override void _Ready()
	{
		Hide();
		ProcessMode = ProcessModeEnum.Always;

		GetNode<Button>("CenterContainer/VBoxContainer/ResumeButton").Pressed += OnResumePressed;
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

	private void OnQuitToMenuPressed()
	{
		RandomSceneLoader.Instance?.Reset();
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://Scenes/Menus/MainMenu.tscn");
	}
}
