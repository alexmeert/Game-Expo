using Godot;

public partial class LevelController : Node, IEnemyTracker
{
    [Export] public Node2D Chest;
    [Export] public AudioStreamPlayer2D MusicPlayer;

    private int _aliveEnemies = 0;
    private bool _chestRevealed = false;
    private bool _isBossLevel = false;

    public override void _Ready()
    {
        if (Chest != null)
            Chest.Visible = false;

        PlayMusic(_isBossLevel);
    }

    public void SetBoss(bool boss)
    {
        _isBossLevel = boss;
    }

    public void OnEnemySpawned() => _aliveEnemies++;

    public void OnEnemyDied()
    {
        _aliveEnemies--;

        if (_aliveEnemies <= 0 && !_chestRevealed)
            RevealChest();
    }

    private void RevealChest()
    {
        _chestRevealed = true;

        if (Chest is Chest chestNode)
            chestNode.Reveal();
        else
            GD.PrintErr("Chest reference not assigned or not a Chest node!");
    }

    private void PlayMusic(bool boss)
    {
        if (MusicPlayer == null) return;

        string trackPath = boss
            ? "res://Sounds/Music/Boss/BossMusic.mp3"
            : "res://Sounds/Music/Normal/NormalMusic.mp3";

        if (MusicPlayer.Stream?.ResourcePath == trackPath) return;

        var stream = ResourceLoader.Load<AudioStream>(trackPath);
        if (stream == null)
        {
            GD.PrintErr($"Could not load music: {trackPath}");
            return;
        }

        MusicPlayer.Stream = stream;
        MusicPlayer.Play();
    }
}
