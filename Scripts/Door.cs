using Godot;
using System;

public partial class Door : StaticBody2D
{
	[Export] public AnimatedSprite2D DoorSprite;

	private CollisionShape2D _collisionShape;
	private Area2D _triggerArea;

	public override void _Ready()
	{
		_collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		_triggerArea = GetNodeOrNull<Area2D>("TriggerArea");

		if (_triggerArea != null)
			_triggerArea.BodyEntered += OnBodyEntered;
	}

	public void PlayOpenAnimation()
	{
		DoorSprite?.Play("Open");
		if (_collisionShape != null)
			_collisionShape.Disabled = true;
	}

	private async void OnBodyEntered(Node2D body)
	{
		if (body is Player)
		{
			GD.Print("Player entered door area → loading next room");

			if (ScreenFader.Instance != null)
				await ScreenFader.Instance.FadeOut(); // fade to black

			RandomSceneLoader.Instance?.LoadNextRoom();
		}
	}


}
