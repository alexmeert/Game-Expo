using Godot;
using System;

public partial class DeathMenu : Control
{

	public override void _Ready()
	{
		// Hook up buttons (adjust node paths if needed)
		GetNode<Button>("VBoxContainer/MenuButton").Pressed += OnMainMenuPressed;
		GetNode<Button>("VBoxContainer/QuitButton").Pressed += OnQuitPressed;
	}

	private void OnMainMenuPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/Menus/MainMenu.tscn");
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}
}
