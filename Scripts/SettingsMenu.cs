using Godot;
using System;

public partial class SettingsMenu : Control
{
    // Export Nodes for easy access in the inspector
    [Export] private Slider MasterSlider;
    [Export] private Slider MusicSlider;
    [Export] private Slider SFXSlider;

    public override void _Ready()
    {
        GetNode<Button>("HomeButton").Pressed += OnHomePressed;

        // Connect sliders to volume change method
        MasterSlider.ValueChanged += OnMasterVolumeChanged;
        MusicSlider.ValueChanged += OnMusicVolumeChanged;
        SFXSlider.ValueChanged += OnSFXVolumeChanged;

        // Initialize sliders to current bus volumes
        MasterSlider.Value = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master"));
        MusicSlider.Value = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Music"));
        SFXSlider.Value = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("SFX"));
    }

    private void OnHomePressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Menus/MainMenu.tscn");
    }

    private void OnMasterVolumeChanged(double value)
    {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), (float)value);
    }

    private void OnMusicVolumeChanged(double value)
    {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), (float)value);
    }

    private void OnSFXVolumeChanged(double value)
    {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), (float)value);
    }
}
