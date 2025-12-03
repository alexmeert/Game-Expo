using Godot;
using System;

public partial class SettingsMenu : Control
{
    public override void _Ready()
	{
		GetNode<Button>("HomeButton").Pressed += OnHomePressed;
	}

    private void OnHomePressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Menus/MainMenu.tscn");
    }
}
