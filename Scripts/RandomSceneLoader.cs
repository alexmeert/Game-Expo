using Godot;
using System;
using System.Collections.Generic;

public partial class RandomSceneLoader : Node
{
	public static RandomSceneLoader Instance { get; private set; }

	[Export] public string Difficulty0RoomsFolder = "res://Scenes/Levels/Difficulty0";
	[Export] public string Difficulty1RoomsFolder = "res://Scenes/Levels/Difficulty1";
	[Export] public string Difficulty2RoomsFolder = "res://Scenes/Levels/Difficulty2";
	[Export] public string Difficulty3RoomsFolder = "res://Scenes/Levels/Difficulty3";
	[Export] public string Difficulty4RoomsFolder = "res://Scenes/Levels/Difficulty4";
	[Export] public string BossRoomsFolder = "res://Scenes/BossLevels/";
	[Export] public AudioStreamPlayer MusicPlayer;

	private readonly List<List<string>> _difficultyDecks = new();
	private readonly List<string> _bossDeck = new();

	private int _currentDifficulty = 0;
	private bool _isBossNext = false;

	public override void _Ready()
	{
		Instance = this;
		GD.Randomize();

		_difficultyDecks.Add(LoadAndShuffle(Difficulty0RoomsFolder));
		_difficultyDecks.Add(LoadAndShuffle(Difficulty1RoomsFolder));
		_difficultyDecks.Add(LoadAndShuffle(Difficulty2RoomsFolder));
		_difficultyDecks.Add(LoadAndShuffle(Difficulty3RoomsFolder));
		_difficultyDecks.Add(LoadAndShuffle(Difficulty4RoomsFolder));

		LoadSceneList(BossRoomsFolder, _bossDeck);
		Shuffle(_bossDeck);

		if (MusicPlayer != null)
			MusicPlayer.ProcessMode = ProcessModeEnum.Always;
	}

	private List<string> LoadAndShuffle(string folder)
	{
		var deck = new List<string>();
		LoadSceneList(folder, deck);
		Shuffle(deck);
		return deck;
	}

	private void LoadSceneList(string folder, List<string> list)
	{
		list.Clear();
		var dir = DirAccess.Open(folder);

		if (dir == null)
		{
			GD.PrintErr($"Could not open folder: {folder}");
			return;
		}

		dir.ListDirBegin();
		string file;

		while ((file = dir.GetNext()) != "")
		{
			if (!dir.CurrentIsDir() && file.EndsWith(".tscn"))
				list.Add(folder + (folder.EndsWith("/") ? "" : "/") + file);
		}

		dir.ListDirEnd();

		if (list.Count == 0)
			GD.PrintErr($"No scenes found in folder: {folder}");
	}

	private void Shuffle(List<string> list)
	{
		for (int i = list.Count - 1; i > 0; i--)
		{
			int j = (int)GD.Randi() % (i + 1);
			(list[i], list[j]) = (list[j], list[i]);
		}
	}

	public void LoadNextRoom()
	{
		if (_currentDifficulty <= 4)
		{
			var deck = _difficultyDecks[_currentDifficulty];

			if (deck.Count == 0)
			{
				GD.Print($"Difficulty {_currentDifficulty} deck empty — reshuffling.");
				string folder = _currentDifficulty switch
				{
					0 => Difficulty0RoomsFolder,
					1 => Difficulty1RoomsFolder,
					2 => Difficulty2RoomsFolder,
					3 => Difficulty3RoomsFolder,
					4 => Difficulty4RoomsFolder,
					_ => ""
				};

				LoadSceneList(folder, deck);
				Shuffle(deck);
			}

			LoadNextFromDeck(deck, false);
			_currentDifficulty++;
		}
		else
		{
			if (_bossDeck.Count == 0)
			{
				GD.Print("Boss deck empty — reshuffling.");
				LoadSceneList(BossRoomsFolder, _bossDeck);
				Shuffle(_bossDeck);
			}

			LoadNextFromDeck(_bossDeck, true);
			_currentDifficulty = 0;
		}
	}

	private void LoadNextFromDeck(List<string> deck, bool isBoss)
	{
		string path = deck[0];
		deck.RemoveAt(0);

		_isBossNext = isBoss;
		GetTree().ChangeSceneToFile(path);

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
		_currentDifficulty = 0;
		_isBossNext = false;
	
		// Clear decks
		foreach (var deck in _difficultyDecks)
			deck.Clear();
	
		_bossDeck.Clear();
	
		// Reload and reshuffle all decks
		_difficultyDecks[0].AddRange(LoadAndShuffle(Difficulty0RoomsFolder));
		_difficultyDecks[1].AddRange(LoadAndShuffle(Difficulty1RoomsFolder));
		_difficultyDecks[2].AddRange(LoadAndShuffle(Difficulty2RoomsFolder));
		_difficultyDecks[3].AddRange(LoadAndShuffle(Difficulty3RoomsFolder));
		_difficultyDecks[4].AddRange(LoadAndShuffle(Difficulty4RoomsFolder));
	
		LoadSceneList(BossRoomsFolder, _bossDeck);
		Shuffle(_bossDeck);
	}

}
