using Godot;

public partial class LevelController : Node, IEnemyTracker
{
[Export] public Node2D Chest;   // assign in inspector


private int _aliveEnemies = 0;
private bool _chestRevealed = false;

public override void _Ready()
{
	if (Chest != null)
		Chest.Visible = false;
}

public void OnEnemySpawned()
{
	_aliveEnemies++;
}

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
	{
		chestNode.Reveal();
	}
	else
	{
		GD.PrintErr("Chest reference not assigned or not a Chest node!");
	}
}


}
