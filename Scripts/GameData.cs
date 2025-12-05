using Godot;
using System;

public partial class GameData : Node
{
    // Audio volume settings
    public float masterVolume = 1.0f;
    public float musicVolume = 0.8f;
    public float sfxVolume = 0.8f;

    // Default audio volume settings
    public const float defaultMasterVolume = 1.0f;
    public const float defaultMusicVolume = 1.0f;
    public const float defaultSfxVolume = 1.0f;
}
