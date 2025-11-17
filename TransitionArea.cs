using Godot;

public partial class TransitionArea : Area2D
{
	[Export] public NodePath SceneLoaderPath;
	private RandomSceneLoader _loader;

	public override void _Ready()
	{
		_loader = GetNode<RandomSceneLoader>(SceneLoaderPath);
	}

	private void OnBodyEntered(Node body)
	{
		if (body.Name == "Player")
			_loader.LoadNextRoom();   
	}
}
