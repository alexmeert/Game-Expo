using Godot;
using System;

public partial class Chest : Node2D
{
    [Export] public AudioStreamPlayer2D OpenSound;
    [Export] public Sprite2D ClosedSprite;
    [Export] public Sprite2D OpenSprite;

    private bool _isPlayerInRange = false;
    private bool _isOpened = false;

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

        var reward = RollReward();
        GD.Print("Player received: " + reward);

        // TODO: Give the player the upgrade
    }

    private string RollReward()
    {
        string[] rewards = new string[]
        {
            "GPU Upgrade",
            "Case Upgrade",
            "RAM Upgrade",
            "Memory Upgrade",
            "Upgrade All"
        };

        int index = GD.RandRange(0, rewards.Length);
        return rewards[index];
    }

    private void _on_Area2D_body_entered(Node body)
    {
        if (body is Player)
        {
            _isPlayerInRange = true;
        }
    }

    private void _on_Area2D_body_exited(Node body)
    {
        if (body is Player)
        {
            _isPlayerInRange = false;
        }
    }
}
