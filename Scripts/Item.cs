using Godot;
using System;

public partial class Item : Node
{
    [Export] public string ItemName = "";
    [Export] public string Description = "";
    [Export] public Texture2D Icon;

    public virtual void Apply(Player player)
    {
        // Override in child classes
    }

    public virtual void Remove(Player player)
    {
        // Override in child classes if needed
    }
}
