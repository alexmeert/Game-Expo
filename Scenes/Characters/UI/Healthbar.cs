using Godot;

public partial class Healthbar : ProgressBar
{
    private Timer timer;
    private ProgressBar damageBar;

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

    private void SetHealth(float newHealth)
    {
        float prevHealth = health;
        health = Mathf.Clamp(newHealth, 0, MaxValue);

        Value = health;

        if (health <= 0)
        {
            QueueFree();
            return;
        }

        if (health < prevHealth)
        {
            timer.Start();
        }
        else
        {
            damageBar.Value = health;
        }
    }

    public void InitHealth(float startingHealth)
    {
        health = startingHealth;
        MaxValue = health;
        Value = health;

        damageBar.MaxValue = health;
        damageBar.Value = health;
    }

    private void OnTimerTimeout()
    {
        damageBar.Value = health;
    }

    private void OnTimerTimeout()
    {
        damageBar.Value = health;
    }
}
