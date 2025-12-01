using Godot;
using System;
using System.Linq;

public partial class PlayerSpawnPoint : Node2D
{
	[Export] public PackedScene PlayerScene; // Optional: if player needs to be spawned
	[Export] public bool SpawnOnReady = true; // Automatically spawn/move player when scene loads
	[Export] public int Priority = 0; // Higher priority spawn points are used first

	private static PlayerSpawnPoint _activeSpawnPoint;

	public override void _Ready()
	{
		base._Ready();

		if (SpawnOnReady)
		{
			SpawnOrMovePlayer();
		}
	}

	/// <summary>
	/// Spawns a new player or moves existing player to this spawn point
	/// </summary>
	public void SpawnOrMovePlayer()
	{
		var existingPlayer = FindExistingPlayer();

		if (existingPlayer != null)
		{
			// Player already exists, just move it to this spawn point
			existingPlayer.GlobalPosition = GlobalPosition;
			GD.Print($"PlayerSpawnPoint: Moved existing player to {GlobalPosition}");
		}
		else if (PlayerScene != null)
		{
			// Spawn a new player from the scene
			SpawnPlayer();
		}
		else
		{
			GD.PrintErr("PlayerSpawnPoint: No existing player found and PlayerScene is not set!");
		}
	}

	/// <summary>
	/// Finds an existing player in the scene
	/// </summary>
	private Player FindExistingPlayer()
	{
		var scene = GetTree().CurrentScene;
		if (scene == null)
			return null;

		// Try to find player by name first
		var player = scene.GetNodeOrNull<Player>("Player");
		if (player != null)
			return player;

		// Try alternative name
		player = scene.GetNodeOrNull<Player>("MainCharacter");
		if (player != null)
			return player;

		// Search all children for Player type
		var players = scene.GetChildren().OfType<Player>();
		if (players.Any())
		{
			return players.First();
		}

		return null;
	}

	/// <summary>
	/// Spawns a new player instance at this spawn point
	/// </summary>
	private void SpawnPlayer()
	{
		if (PlayerScene == null)
		{
			GD.PrintErr("PlayerSpawnPoint: Cannot spawn player - PlayerScene is null!");
			return;
		}

		var player = PlayerScene.Instantiate<Player>();
		if (player == null)
		{
			GD.PrintErr("PlayerSpawnPoint: Failed to instantiate player from scene!");
			return;
		}

		player.GlobalPosition = GlobalPosition;

		var parent = GetParent();
		if (parent != null)
		{
			parent.AddChild(player);
			GD.Print($"PlayerSpawnPoint: Spawned player at {GlobalPosition}");
		}
		else
		{
			// Fallback: add to current scene
			var scene = GetTree().CurrentScene;
			if (scene != null)
			{
				scene.AddChild(player);
				GD.Print($"PlayerSpawnPoint: Spawned player at {GlobalPosition} (added to scene root)");
			}
			else
			{
				GD.PrintErr("PlayerSpawnPoint: Cannot spawn player - no valid parent!");
				player.QueueFree();
			}
		}
	}

	/// <summary>
	/// Static method to find and use the highest priority spawn point in a given scene
	/// </summary>
	public static void SpawnPlayerAtBestPoint(Node scene)
	{
		if (scene == null)
			return;

		// Find all spawn points
		var spawnPoints = scene.GetChildren().OfType<PlayerSpawnPoint>()
			.OrderByDescending(sp => sp.Priority)
			.ToList();

		if (spawnPoints.Count == 0)
		{
			GD.Print("No PlayerSpawnPoint found in scene");
			return;
		}

		// Use the highest priority spawn point
		spawnPoints[0].SpawnOrMovePlayer();
	}
}
