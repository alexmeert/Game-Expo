using Godot;

public partial class TransitionArea : Area2D
{
	private void OnBodyEntered(Node body)
	{
		if (body.Name == "MainCharacter")
		{
			RandomSceneLoader.Instance.LoadNextRoom();
		}
	}
}
