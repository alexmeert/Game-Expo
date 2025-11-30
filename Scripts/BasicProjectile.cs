using Godot;

public partial class BasicProjectile : Area2D
{
    [Export] public float SPD = 400f;
    [Export] public float DMG = 10f;

    public Vector2 Direction = Vector2.Up;
    public BasicEntity Owner { get; set; }

    public override void _Ready()
    {
        Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
    }

    public override void _PhysicsProcess(double delta)
    {
        Position += Direction * SPD * (float)delta;
    }

    private void OnBodyEntered(Node body)
    {    
        if (body is BasicEntity entity && entity != Owner)
        {
            entity.TakeDamage(DMG);
            QueueFree();
        }
    }

    public void SetDirection(Vector2 dir)
    {
        Direction = dir.Normalized();
    }

    public void SetDamage(float dmg)
    {
        DMG = dmg;
    }
}
