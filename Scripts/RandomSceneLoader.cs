using Godot;
using System;
using System.Collections.Generic;

public partial class RandomSceneLoader : Node
{
	public static RandomSceneLoader Instance { get; private set; }

	[Export] public string LevelsFolder = "res://Scenes/Levels";
	[Export] public string BossLevelPath = "res://Scenes/BossLevels";
	[Export] public AudioStreamPlayer MusicPlayer;

	private List<string> _randomLevelOrder = new List<string>();
	private int _currentIndex = 0;
	private bool _isBossNext = false;

	public override void _Ready()
	{
		Instance = this;
		GD.Randomize();

		if (MusicPlayer != null)
			MusicPlayer.ProcessMode = ProcessModeEnum.Always;

		GenerateRandomOrder();
	}

	private void GenerateRandomOrder()
	{
		// Create list of 5 levels
		_randomLevelOrder = new List<string>
		{
			$"{LevelsFolder}/Level1.tscn",
			$"{LevelsFolder}/Level2.tscn",
			$"{LevelsFolder}/Level3.tscn",
			$"{LevelsFolder}/Level4.tscn",
			$"{LevelsFolder}/Level5.tscn"
		};

		// Shuffle
		for (int i = _randomLevelOrder.Count - 1; i > 0; i--)
		{
			int j = (int)(GD.Randi() % (ulong)(i + 1));
			(_randomLevelOrder[i], _randomLevelOrder[j]) = (_randomLevelOrder[j], _randomLevelOrder[i]);
		}


		GD.Print("Random level order:");
		foreach (var lv in _randomLevelOrder)
			GD.Print(" - " + lv);

		_currentIndex = 0;
		_isBossNext = false;
	}

	public void LoadNextRoom()
	{
		string scenePath;
		bool isBoss = false;

		// If we still have randomized levels left
		if (_currentIndex < _randomLevelOrder.Count)
		{
			scenePath = _randomLevelOrder[_currentIndex];
			_currentIndex++;
			isBoss = false;
		}
		else
		{
			// Load a boss level
			scenePath = LoadBossLevel();
			isBoss = true;
		}

		_isBossNext = isBoss;

		GD.Print($"Loading: {scenePath} (Boss: {isBoss})");
		GetTree().ChangeSceneToFile(scenePath);
		CallDeferred(nameof(ApplyBossFlag));
	}

	private string LoadBossLevel()
	{
		return $"{BossLevelPath}/BossLevel1.tscn";
	}


	public void Reset()
	{
		GenerateRandomOrder();
	}

	private void ApplyBossFlag()
	{
		var level = GetTree().CurrentScene?.GetNodeOrNull<LevelController>(".");
		if (level != null)
			level.SetBoss(_isBossNext);
		else
			GD.PrintErr("Loaded level root has no LevelController!");
	}
}
