using Godot;

public partial class SpeakerGun : Node2D
{
    [Export] public Marker2D Muzzle;
    [Export] public SpeakerProjectileManager SpeakerProjectileManager;
    [Export] public AudioStreamPlayer2D ShotSound;
    [Export] public float AttackCooldown = 1.5f;
    [Export] public bool IsArcAttacker = false; // false = left speaker (targeted), true = right speaker (arc)

    private float attackTimer = 0f;
    private Player targetPlayer;

    public BasicEntity Owner { get; set; }

    public override void _Ready()
    {
        FindPlayer();
        
        // Auto-find SpeakerProjectileManager if not assigned
        if (SpeakerProjectileManager == null)
        {
            SpeakerProjectileManager = GetNodeOrNull<SpeakerProjectileManager>("SpeakerProjectileManager");
        }
        
        // Auto-find Muzzle if not assigned
        if (Muzzle == null)
        {
            Muzzle = GetNodeOrNull<Marker2D>("Muzzle");
        }
        
        // Start with a small delay before first attack
        attackTimer = 0.5f;
        
        // Debug: Check if components are set up
        if (SpeakerProjectileManager == null)
        {
            GD.PrintErr("SpeakerGun: SpeakerProjectileManager is not assigned or found!");
        }
        if (Owner == null)
        {
            GD.PrintErr("SpeakerGun: Owner is not set!");
        }
        if (Muzzle == null)
        {
            GD.PrintErr("SpeakerGun: Muzzle (Marker2D) is not assigned or found!");
        }
    }

    public override void _Process(double delta)
    {
        if (attackTimer > 0)
            attackTimer -= (float)delta;

        if (targetPlayer == null || !targetPlayer.IsAlive)
        {
            FindPlayer();
            return;
        }

        if (attackTimer <= 0f && Owner != null && Owner.IsAlive)
        {
            TryFire();
        }
    }

    private void FindPlayer()
    {
        var scene = GetTree().CurrentScene;
        if (scene != null)
        {
            // MainCharacter is always a CharacterBody2D called "MainCharacter"
            var mainCharacter = scene.GetNodeOrNull<CharacterBody2D>("MainCharacter");
            if (mainCharacter is Player player)
            {
                targetPlayer = player;
            }
        }
    }

    private void TryFire()
    {
        if (SpeakerProjectileManager == null)
        {
            GD.PrintErr("SpeakerGun: SpeakerProjectileManager is null!");
            return;
        }

        if (Owner == null || !Owner.IsAlive)
        {
            GD.PrintErr("SpeakerGun: Owner is null or not alive!");
            return;
        }

        if (targetPlayer == null || !targetPlayer.IsAlive)
        {
            GD.PrintErr("SpeakerGun: TargetPlayer is null or not alive!");
            return;
        }

        Vector2 spawnPosition = Muzzle != null ? Muzzle.GlobalPosition : Owner.GlobalPosition;
        Vector2 targetPosition = targetPlayer.GlobalPosition;

        if (IsArcAttacker)
        {
            // Right speaker: 5 projectiles in arc
            SpeakerProjectileManager.SpawnArcProjectiles(spawnPosition, targetPosition, 45f);
        }
        else
        {
            // Left speaker: single targeted projectile
            SpeakerProjectileManager.SpawnTargetedProjectile(spawnPosition, targetPosition);
        }

        attackTimer = AttackCooldown;
        ShotSound?.Play();
    }
}

