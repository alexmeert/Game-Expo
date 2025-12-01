using Godot;
using System;
using System.Collections.Generic;

public partial class Player : BasicEntity
{
	private const float ACCEL = 15.0f;
	private const float FRICTION = 12.0f;

    [Export] private int Hp = 100;
    [Export] private int Dmg = 10;
    [Export] private float AtkSpd = 1.0f;
    [Export] private float Def = 0f; // Percentage (0.0 = 0%, 1.0 = 100% damage reduction)
    [Export] private float Spd = 100;
    [Export] private AudioStreamPlayer2D HitSound;
    [Export] private AudioStreamPlayer2D DeathSound;

    private List<Upgrade> _activeUpgrades = new List<Upgrade>();
    private List<Perk> _activePerks = new List<Perk>();

    // Base stats (before upgrades) - used for percentage calculations
    private float _baseMaxHP;
    private float _baseDMG;
    private float _baseATKSPD;
    private float _baseDEF;
    private float _baseSPD;

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

        // Store base stats for percentage calculations
        _baseMaxHP = MaxHP;
        _baseDMG = DMG;
        _baseATKSPD = ATKSPD;
        _baseDEF = DEF;
        _baseSPD = SPD;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        UpdatePerks((float)delta);
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
        
        GetTree().ChangeSceneToFile("res://Scenes/Menus/DeathMenu.tscn");
    }

    /// <summary>
    /// Applies a permanent upgrade to the player
    /// </summary>
    public void ApplyUpgrade(Upgrade upgrade)
    {
        if (upgrade == null)
            return;

        upgrade.Apply(this);
        _activeUpgrades.Add(upgrade);
    }

    /// <summary>
    /// Applies a temporary perk to the player
    /// </summary>
    public void ApplyPerk(Perk perk)
    {
        if (perk == null)
            return;

        // If perk is not in the list, add it
        if (!_activePerks.Contains(perk))
        {
            _activePerks.Add(perk);
        }

        // Apply or refresh the perk
        perk.Apply(this);
    }

    /// <summary>
    /// Updates all active perks and removes expired ones
    /// </summary>
    private void UpdatePerks(float delta)
    {
        for (int i = _activePerks.Count - 1; i >= 0; i--)
        {
            Perk perk = _activePerks[i];
            perk.Update(delta);

            if (!perk.IsActive)
            {
                perk.Remove(this);
                _activePerks.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Gets all active upgrades
    /// </summary>
    public List<Upgrade> GetActiveUpgrades()
    {
        return new List<Upgrade>(_activeUpgrades);
    }

    /// <summary>
    /// Gets all active perks
    /// </summary>
    public List<Perk> GetActivePerks()
    {
        return new List<Perk>(_activePerks);
    }

    // Base stat getters for upgrade percentage calculations
    public float GetBaseMaxHP() => _baseMaxHP;
    public float GetBaseDMG() => _baseDMG;
    public float GetBaseATKSPD() => _baseATKSPD;
    public float GetBaseDEF() => _baseDEF;
    public float GetBaseSPD() => _baseSPD;
}
