using Godot;
using System;

public partial class RandomSceneLoader : Node
{
	public static RandomSceneLoader Instance { get; private set; }

	[Export] public string LevelsFolder = "res://Scenes/Levels";
	[Export] public string BossLevelPath = "res://Scenes/BossLevels/";
	[Export] public AudioStreamPlayer MusicPlayer;

	private int _currentLevel = 0; // 0 = not started, 1-5 = levels, 6+ = boss
	private bool _isBossNext = false;

	public override void _Ready()
	{
		Instance = this;
		GD.Randomize();

		if (MusicPlayer != null)
			MusicPlayer.ProcessMode = ProcessModeEnum.Always;
	}

	public void LoadNextRoom()
	{
		string scenePath;
		bool isBoss = false;

		if (_currentLevel < 5)
		{
			// Load Level1, Level2, Level3, Level4, or Level5
			_currentLevel++;
			scenePath = $"{LevelsFolder}/Level{_currentLevel}.tscn";
			isBoss = false;
		}
		else
		{
			// After level 5, load boss level
			// Try to find boss level in the BossLevels folder
			var dir = DirAccess.Open(BossLevelPath);
			if (dir != null)
			{
				dir.ListDirBegin();
				string file;
				string bossFile = null;
				
				while ((file = dir.GetNext()) != "")
				{
					if (!dir.CurrentIsDir() && file.EndsWith(".tscn"))
					{
						bossFile = file;
						break; // Use first boss scene found
					}
				}
				dir.ListDirEnd();
				
				if (bossFile != null)
				{
					scenePath = BossLevelPath + (BossLevelPath.EndsWith("/") ? "" : "/") + bossFile;
				}
				else
				{
					GD.PrintErr($"No boss level found in: {BossLevelPath}");
					scenePath = $"{LevelsFolder}/Level5.tscn"; // Fallback
				}
			}
			else
			{
				GD.PrintErr($"Could not open boss folder: {BossLevelPath}");
				scenePath = $"{LevelsFolder}/Level5.tscn"; // Fallback
			}
			
			isBoss = true;
		}

		_isBossNext = isBoss;
		GD.Print($"Loading: {scenePath} (Boss: {isBoss})");
		
		GetTree().ChangeSceneToFile(scenePath);
		CallDeferred(nameof(ApplyBossFlag));
	}

	private void ApplyBossFlag()
	{
		var level = GetTree().CurrentScene?.GetNodeOrNull<LevelController>(".");
		if (level != null)
			level.SetBoss(_isBossNext);
		else
			GD.PrintErr("Loaded level root has no LevelController!");
	}

	public void Reset()
	{
		_currentLevel = 0;
		_isBossNext = false;
	}
}
