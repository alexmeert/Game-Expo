using Godot;
using System;
using System.Linq;

public partial class MeleeEnemy : BasicEntity
{
    [Export] private int Hp = 50;
    [Export] private int Dmg = 1;
    [Export] private float AtkSpd = 1.2f;
    [Export] private int Def = 1;
    [Export] private float Spd = 20;
    [Export] private AudioStreamPlayer2D HitSound;
    [Export] private float AttackRange = 30f;

    protected Node2D TargetPlayer { get; private set; }
    private float _attackTimer = 0f;

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

        FindPlayer();
    }

	protected override void HandleMovement(double delta)
	{
		if (TargetPlayer == null || !TargetPlayer.IsInsideTree())
		{
			FindPlayer();
			if (TargetPlayer == null)
				return;
		}

        float distance = GlobalPosition.DistanceTo(TargetPlayer.GlobalPosition);
        
        // Attack logic
        _attackTimer -= (float)delta;
        if (_attackTimer <= 0f && distance <= AttackRange)
        {
            AttackPlayer();
            _attackTimer = 1.0f / ATKSPD;
        }
        
        // Move toward player if not in attack range
        if (distance > AttackRange)
        {
            Vector2 direction = (TargetPlayer.GlobalPosition - GlobalPosition).Normalized();
            Velocity = direction * SPD;
        }
        else
        {
            // Stop moving when in attack range
            Velocity = Vector2.Zero;
        }
    }

    private void AttackPlayer()
    {
        if (TargetPlayer is Player player && player.IsAlive)
        {
            player.TakeDamage(DMG);
            GD.Print($"MeleeEnemy dealt {DMG} damage to player. Player HP: {player.HP}/{player.MaxHP}");
        }
    }

    protected virtual void FindPlayer()
    {
        var scene = GetTree().CurrentScene;
        if (scene != null)
        {
            // Try to find player by name first
            TargetPlayer = scene.GetNodeOrNull<Node2D>("MainCharacter");
            
            // If not found, search for Player node by type
            if (TargetPlayer == null)
            {
                TargetPlayer = scene.GetNodeOrNull<Player>("Player");
            }
            
            // Last resort: search all children for Player type
            if (TargetPlayer == null)
            {
                var players = scene.GetChildren().OfType<Player>();
                if (players.Any())
                {
                    TargetPlayer = players.First();
                }
            }
        }
    }

    protected override void OnTakeDamage(float damage)
    {
        base.OnTakeDamage(damage);
        
        // Play hit sound when enemy is hit by a projectile
        if (HitSound != null)
        {
            HitSound.Play();
        }
    }
}
