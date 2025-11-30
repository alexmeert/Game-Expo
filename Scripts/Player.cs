using Godot;
using System;

public partial class Player : BasicEntity
{
	private const float ACCEL = 15.0f;
	private const float FRICTION = 12.0f;

    [Export] private int Hp = 100;
    [Export] private int Dmg = 10;
    [Export] private float AtkSpd = 1.0f;
    [Export] private int Def = 0;
    [Export] private float Spd = 100;
    [Export] private AudioStreamPlayer2D HitSound;
    [Export] private AudioStreamPlayer2D DeathSound;

	public override void _Ready()
	{
		base._Ready();

		var gun = GetNode<Gun>("Gun");
		if (gun != null)
			gun.Owner = this;
	}

    protected override void InitializeEntity()
    {
        base.InitializeEntity();

        SetStats(
            hp: Hp,
            dmg: Dmg,
            atkspd: AtkSpd,
            def: Def,
            spd: Spd
        );
    }

    protected override void HandleMovement(double delta)
    {
        Vector2 input = GetInput();

        // Update velocity
        if (input.Length() > 0)
            Velocity = Velocity.Lerp(input * Spd, (float)delta * ACCEL);
        else
            Velocity = Velocity.Lerp(Vector2.Zero, (float)delta * FRICTION);

        // Move the player
        Position += Velocity * (float)delta;

        // Handle animation
        var animSprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");
        if (animSprite != null)
        {
            if (input.Length() == 0)
            {
                animSprite.Play("Idle");
            }
            else
            {
                if (Mathf.Abs(input.X) > Mathf.Abs(input.Y))
                {
                    animSprite.Play(input.X > 0 ? "MoveRight" : "MoveLeft");
                }
                else
                {
                    animSprite.Play(input.Y > 0 ? "MoveDown" : "MoveUp");
                }
            }
        }
    }



    private Vector2 GetInput()
    {
        float inputX = Input.GetActionStrength("D") - Input.GetActionStrength("A");
        float inputY = Input.GetActionStrength("S") - Input.GetActionStrength("W");

		Vector2 vec = new Vector2(inputX, inputY);

        return vec.Length() > 0 ? vec.Normalized() : Vector2.Zero;
    }

    protected override void OnTakeDamage(float damage)
    {
        base.OnTakeDamage(damage);
        
        // Play hit sound when player takes damage
        if (HitSound != null)
        {
            HitSound.Play();
        }
        
        GD.Print($"Player took {damage} damage. HP: {HP}/{MaxHP}");
    }

    protected override void Die()
    {
        base.Die();
        
        // Play death sound
        if (DeathSound != null)
        {
            DeathSound.Play();
        }
        
        GD.Print("Player died!");
        
        // TODO: Add game over logic here (e.g., show game over screen, restart level, etc.)
    }
}
