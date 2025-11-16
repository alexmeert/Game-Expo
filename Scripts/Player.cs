using Godot;
using System;

public partial class Player : BasicEntity
{
    private const float ACCEL = 15.0f;
    private const float FRICTION = 12.0f;

    [Export] private Sprite2D sprite;

    public override void _Ready()
    {
        base._Ready();

        var gun = GetNode<Gun>("Gun");
        gun.Owner = this;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Flip sprite based on mouse direction
        if (GetGlobalMousePosition().X < GlobalPosition.X)
            sprite.FlipH = true;
        else
            sprite.FlipH = false;

        Vector2 input = GetInput();

        if (input.Length() > 0)
            Velocity = Velocity.Lerp(input * SPD, (float)delta * ACCEL);
        else
            Velocity = Velocity.Lerp(Vector2.Zero, (float)delta * FRICTION);

        MoveAndSlide();
    }

    private Vector2 GetInput()
    {
        float inputX = Input.GetActionStrength("D") - Input.GetActionStrength("A");
        float inputY = Input.GetActionStrength("S") - Input.GetActionStrength("W");

        Vector2 vec = new Vector2(inputX, inputY);

        return vec.Length() > 0 ? vec.Normalized() : Vector2.Zero;
    }
}
