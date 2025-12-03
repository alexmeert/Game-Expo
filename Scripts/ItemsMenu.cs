using Godot;
using System;

public partial class ItemsMenu : Control
{
    private CaroselContainer _upgradeCarousel;
    private CaroselContainer _perksCarousel;

    public override void _Ready()
    {
        GetNode<Button>("HomeButton").Pressed += OnHomePressed;
        GetNode<Button>("NextUpgradeButton").Pressed += OnNextUpgradePressed;
        GetNode<Button>("PreviousUpgradeButton").Pressed += OnPreviousUpgradePressed;
        GetNode<Button>("NextPerkButton").Pressed += OnNextPerkPressed;
        GetNode<Button>("PreviousPerkButton").Pressed += OnPreviousPerkPressed;

        _upgradeCarousel = GetNode<CaroselContainer>("UpgradesContainer/UpgradesCaroselContainer");
        _perksCarousel = GetNode<CaroselContainer>("PerksContainer/PerksCaroselContainer");

        _upgradeCarousel.PositionOffsetNode = GetNode<Godot.Control>("UpgradesContainer/UpgradesCaroselContainer/Control");
        _perksCarousel.PositionOffsetNode = GetNode<Godot.Control>("PerksContainer/PerksCaroselContainer/Control");
    }

    private void OnHomePressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Menus/MainMenu.tscn");
    }

    private void OnNextUpgradePressed()
    {
        _upgradeCarousel.Right();
        GrabChildFocus(_upgradeCarousel);
    }

    private void OnPreviousUpgradePressed()
    {
        _upgradeCarousel.Left();
        GrabChildFocus(_upgradeCarousel);
    }

    private void OnNextPerkPressed()
    {
        _perksCarousel.Right();
        GrabChildFocus(_perksCarousel);
    }

    private void OnPreviousPerkPressed()
    {
        _perksCarousel.Left();
        GrabChildFocus(_perksCarousel);
    }

    private void GrabChildFocus(CaroselContainer carousel)
    {
        var selected = carousel.PositionOffsetNode?.GetChild<Control>(carousel.SelectedIndex);
        selected?.GrabFocus();
    }
}
