using Godot;
using System;

public partial class DeathMenu : Control
{
	private const string MainMenuScenePath = "res://Scenes/Menus/MainMenu.tscn";

	public override void _Ready()
	{
		// Hook up buttons (adjust node paths if needed)
		GetNode<Button>("VBoxContainer/MainMenuButton").Pressed += OnMainMenuPressed;
		GetNode<Button>("VBoxContainer/QuitButton").Pressed += OnQuitPressed;
	}

	private void OnMainMenuPressed()
	{
		GetTree().ChangeSceneToFile(MainMenuScenePath);
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}
}
