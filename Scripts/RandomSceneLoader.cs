using Godot;
using System.Collections.Generic;

public partial class RandomSceneLoader : Node
{
	public static RandomSceneLoader Instance { get; private set; }

	[Export] public string NormalRoomsFolder = "res://Scenes/Levels/";
	[Export] public string BossRoomsFolder = "res://Scenes/BossLevels/";
	[Export] public int RoomsPerCycle = 3;

	private List<string> _normalRooms = new();
	private List<string> _bossRooms = new();

	private int _roomsCleared = 0;

	public override void _Ready()
	{
		Instance = this;

		GD.Randomize();

		LoadSceneList(NormalRoomsFolder, _normalRooms);
		LoadSceneList(BossRoomsFolder, _bossRooms);

		Shuffle(_normalRooms);
		Shuffle(_bossRooms);
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
		string fileName;

		while ((fileName = dir.GetNext()) != "")
		{
			if (!dir.CurrentIsDir() && fileName.EndsWith(".tscn"))
				list.Add(folder + fileName);
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
		if (_roomsCleared < RoomsPerCycle)
		{
			LoadNextFromDeck(_normalRooms);
			_roomsCleared++;
		}
		else
		{
			LoadNextFromDeck(_bossRooms);
			_roomsCleared = 0;
		}
	}

	private void LoadNextFromDeck(List<string> deck)
	{
		if (deck.Count == 0)
		{
			GD.Print("Deck empty — reshuffling.");
			LoadSceneList(deck == _normalRooms ? NormalRoomsFolder : BossRoomsFolder, deck);
			Shuffle(deck);
		}

		string path = deck[0];
		deck.RemoveAt(0);

		GD.Print($"Loading room: {path}");
		GetTree().ChangeSceneToFile(path);
	}
}
