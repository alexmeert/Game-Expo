using Godot;
using System.Collections.Generic;

public partial class RandomSceneLoader : Node
{
	[Export] public string NormalRoomsFolder = "res://Rooms/";
	[Export] public string BossRoomsFolder = "res://BossRooms/";
	[Export] public int RoomsPerCycle = 3;

	private List<string> _normalRooms = new();
	private List<string> _bossRooms = new();

	private int _roomsCleared = 0;

	public override void _Ready()
	{
		LoadSceneList(NormalRoomsFolder, _normalRooms);
		LoadSceneList(BossRoomsFolder, _bossRooms);
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
			{
				list.Add(folder + fileName);
			}
		}

		dir.ListDirEnd();

		if (list.Count == 0)
			GD.PrintErr($"No scenes found in folder: {folder}");
	}

	// Call this when the player finishes a room
	public void LoadNextRoom()
	{
		if (_roomsCleared < RoomsPerCycle)
		{
			// Load normal room
			LoadRandomRoom(_normalRooms);
			_roomsCleared++;
		}
		else
		{
			// Load boss room
			LoadRandomRoom(_bossRooms);
			_roomsCleared = 0; // Reset for next loop
		}
	}

	private void LoadRandomRoom(List<string> pool)
	{
		if (pool.Count == 0)
		{
			GD.PrintErr("Room pool is empty!");
			return;
		}

		int index = GD.RandRange(0, pool.Count - 1);
		string path = pool[index];

		GetTree().ChangeSceneToFile(path);
	}
}
