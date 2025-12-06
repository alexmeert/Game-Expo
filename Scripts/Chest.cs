using Godot;
using System;

public partial class Chest : Node2D
{
	[Export] public AudioStreamPlayer2D OpenSound;
	[Export] public Sprite2D ClosedSprite;
	[Export] public Sprite2D OpenSprite;
	[Export] public PackedScene[] UpgradeScenes = new PackedScene[6];
	[Export] public Vector2 UpgradeSpawnOffset = new Vector2(0, -50);


	private bool _isPlayerInRange = false;
	private bool _isOpened = false;
	private bool _isRevealed = false;

	private Area2D _interactionArea;
	private CollisionShape2D _rootCollisionShape;
	private Door _door;

	public override void _Ready()
	{
		base._Ready();

		Visible = false;

		_rootCollisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		if (_rootCollisionShape != null)
			_rootCollisionShape.Disabled = true;

		_interactionArea = GetNodeOrNull<Area2D>("Area2D") ?? GetNodeOrNull<Area2D>("InteractionArea");
		if (_interactionArea == null)
		{
			foreach (Node child in GetChildren())
			{
				if (child is Area2D area)
				{
					_interactionArea = area;
					break;
				}
			}
		}

		if (_interactionArea != null)
			_interactionArea.Monitoring = false;

		if (_interactionArea != null)
		{
			_interactionArea.BodyEntered += OnBodyEntered;
			_interactionArea.BodyExited += OnBodyExited;
		}


		_door = GetTree().CurrentScene.GetNodeOrNull<Door>("Door");

	}

	public void Reveal()
	{
		_isRevealed = true;
		Visible = true;


		if (_interactionArea != null)
			_interactionArea.CallDeferred("set_monitoring", true);

		var shape = GetNode<CollisionShape2D>("CollisionShape2D");
		if (shape != null)
			shape.CallDeferred("set_disabled", false);

		GD.Print("Chest revealed!");
	}


	public override void _Process(double delta)
	{
		if (_isRevealed && _isPlayerInRange && !_isOpened && Input.IsActionJustPressed("Interact"))
		{
			OpenChest();
		}
	}

	private void OpenChest()
	{
		_isOpened = true;

		if (ClosedSprite != null && OpenSprite != null)
		{
			ClosedSprite.Visible = false;
			OpenSprite.Visible = true;
		}

		OpenSound?.Play();

		_door?.PlayOpenAnimation();

		SpawnUpgrade();
	}


	private void SpawnUpgrade()
	{
		var validUpgrades = new System.Collections.Generic.List<PackedScene>();
		foreach (var scene in UpgradeScenes)
			if (scene != null) validUpgrades.Add(scene);

		if (validUpgrades.Count == 0) return;

		int randomIndex = (int)GD.RandRange(0, validUpgrades.Count - 1);
		PackedScene selectedUpgradeScene = validUpgrades[randomIndex];

		var upgrade = selectedUpgradeScene.Instantiate<Upgrade>();
		if (upgrade == null) return;

		UpgradeRarity rarity = Upgrade.GetRandomRarity();
		upgrade.SetRarity(rarity);
		upgrade.GlobalPosition = GlobalPosition + UpgradeSpawnOffset;

		var parent = GetParent() ?? GetTree().CurrentScene;
		if (parent != null)
		{
			parent.AddChild(upgrade);
		}
		else
		{
			upgrade.QueueFree();
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		if (!_isRevealed) return;
		if (body is Player)
		{
			_isPlayerInRange = true;
		}
	}

	private void OnBodyExited(Node2D body)
	{
		if (body is Player)
		{
			_isPlayerInRange = false;
		}
	}
}
