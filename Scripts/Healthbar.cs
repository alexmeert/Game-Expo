using Godot;

public partial class Healthbar : ProgressBar
{
    private Timer timer;
    private ProgressBar damageBar;
    private BasicEntity targetEntity;

    private float health;
    public float Health
    {
        get => health;
        set => SetHealth(value);
    }

    public override void _Ready()
    {
        timer = GetNode<Timer>("Timer");
        damageBar = GetNode<ProgressBar>("DamageBar");
        timer.Timeout += OnTimerTimeout;
    }

    /// <summary>
    /// Connects the healthbar to a BasicEntity and initializes it
    /// </summary>
    public void ConnectToEntity(BasicEntity entity)
    {
        if (entity == null)
            return;

        targetEntity = entity;
        
        // Initialize with entity's current stats
        InitHealth(entity.MaxHP);
        SetHealth(entity.HP);
        
        // Connect to entity's HP change signal if available
        // Since BasicEntity doesn't have signals, we'll need to update manually
        // or add a reference that can be checked
    }

    /// <summary>
    /// Updates the healthbar to match the entity's current HP and MaxHP
    /// Call this from the entity's OnHPChanged or in _Process
    /// </summary>
    public void UpdateFromEntity()
    {
        if (targetEntity == null)
            return;

        // Update MaxValue if MaxHP changed (e.g., from upgrades)
        if (MaxValue != targetEntity.MaxHP)
        {
            MaxValue = targetEntity.MaxHP;
            damageBar.MaxValue = targetEntity.MaxHP;
        }

        // Update current health
        SetHealth(targetEntity.HP);
    }

    private void SetHealth(float newHealth)
    {
        float prevHealth = health;
        health = Mathf.Clamp(newHealth, 0f, (float)MaxValue);

        Value = health;

        if (health <= 0)
        {
            // Don't queue free if entity is still alive (might be healing)
            // Only hide or show 0
            Value = 0;
            return;
        }

        if (health < prevHealth)
        {
            // Health decreased - start damage bar animation
            timer.Start();
        }
        else if (health > prevHealth)
        {
            // Health increased - immediately update damage bar
            damageBar.Value = health;
        }
    }

    public void InitHealth(float startingHealth)
    {
        health = startingHealth;
        MaxValue = health;
        Value = health;

        if (damageBar != null)
        {
            damageBar.MaxValue = health;
            damageBar.Value = health;
        }
    }


    private void OnTimerTimeout()
    {
        if (damageBar != null)
        {
            damageBar.Value = health;
        }
    }
}
