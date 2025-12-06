using Godot;
using System;

public partial class WinMenu : Control
{
	[Export] private Button MainButton;
	[Export] private Button QuitButton;

	public override async void _Ready()
	{
		
		if (ScreenFader.Instance != null)
			ScreenFader.Instance.FadeIn();

		if (MainButton == null)
			MainButton = GetNode<Button>("MainButton");

		if (QuitButton == null)
			QuitButton = GetNode<Button>("QuitButton");

		MainButton.Pressed += OnMainPressed;
		QuitButton.Pressed += OnQuitPressed;
	}

	private async void OnMainPressed()
	{
		
		if (ScreenFader.Instance != null)
			await ScreenFader.Instance.FadeOut();

		GetTree().ChangeSceneToFile("res://Scenes/Menus/MainMenu.tscn");
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}
}
