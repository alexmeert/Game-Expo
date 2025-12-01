using Godot;
using System;

public partial class Chest : Node2D
{
    [Export] public AudioStreamPlayer2D OpenSound;
    [Export] public Sprite2D ClosedSprite;
    [Export] public Sprite2D OpenSprite;
    [Export] public PackedScene[] UpgradeScenes = new PackedScene[6]; // Array of 6 different upgrade scenes
    [Export] public Vector2 UpgradeSpawnOffset = new Vector2(0, -50); // Offset from chest position

    private bool _isPlayerInRange = false;
    private bool _isOpened = false;
    private Area2D _interactionArea;

    public override void _Ready()
    {
        base._Ready();
        
        // Find the Area2D node (try common names)
        _interactionArea = GetNodeOrNull<Area2D>("Area2D");
        if (_interactionArea == null)
        {
            _interactionArea = GetNodeOrNull<Area2D>("InteractionArea");
        }
        if (_interactionArea == null)
        {
            // Search in children
            foreach (Node child in GetChildren())
            {
                if (child is Area2D area)
                {
                    _interactionArea = area;
                    break;
                }
            }
        }

        if (_interactionArea == null)
        {
            GD.PrintErr("Chest: No Area2D found! Make sure there's an Area2D child node.");
            return;
        }

        // Connect signals
        _interactionArea.BodyEntered += OnBodyEntered;
        _interactionArea.BodyExited += OnBodyExited;
        
        GD.Print("Chest: Signals connected successfully");
    }

    public override void _Process(double delta)
    {
        if (_isPlayerInRange && !_isOpened && Input.IsActionJustPressed("Interact"))
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

        SpawnUpgrade();
    }

    private void SpawnUpgrade()
    {
        // Filter out null upgrade scenes
        var validUpgrades = new System.Collections.Generic.List<PackedScene>();
        foreach (var scene in UpgradeScenes)
        {
            if (scene != null)
            {
                validUpgrades.Add(scene);
            }
        }

        if (validUpgrades.Count == 0)
        {
            return;
        }

        // Randomly select one of the available upgrade scenes
        int randomIndex = (int)GD.RandRange(0, validUpgrades.Count - 1);
        PackedScene selectedUpgradeScene = validUpgrades[randomIndex];

        var upgrade = selectedUpgradeScene.Instantiate<Upgrade>();
        if (upgrade == null)
        {
            return;
        }

        // Set random rarity based on percentages
        UpgradeRarity rarity = Upgrade.GetRandomRarity();
        upgrade.SetRarity(rarity);

        // Position upgrade above the chest
        upgrade.GlobalPosition = GlobalPosition + UpgradeSpawnOffset;

        // Add to scene
        var parent = GetParent();
        if (parent != null)
        {
            parent.AddChild(upgrade);
            GD.Print($"Chest spawned {rarity} upgrade '{upgrade.ItemName}' at {upgrade.GlobalPosition}");
        }
        else
        {
            var scene = GetTree().CurrentScene;
            if (scene != null)
            {
                scene.AddChild(upgrade);
                GD.Print($"Chest spawned {rarity} upgrade '{upgrade.ItemName}' at {upgrade.GlobalPosition}");
            }
            else
            {
                GD.PrintErr("Chest: Cannot spawn upgrade - no valid parent!");
                upgrade.QueueFree();
            }
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            _isPlayerInRange = true;
            GD.Print("Chest: Player entered interaction area");
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Player)
        {
            _isPlayerInRange = false;
            GD.Print("Chest: Player exited interaction area");
        }
    }
}
