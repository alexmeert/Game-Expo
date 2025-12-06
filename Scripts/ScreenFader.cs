using Godot;
using System;
using System.Threading.Tasks;

public partial class ScreenFader : ColorRect
{
	[Export] public float FadeTime = 0.5f; // seconds
	public static ScreenFader Instance;

	public override void _Ready()
	{
		Instance = this;

		// Start fully transparent
		Modulate = new Color(0, 0, 0, 0);
	}


	public async void FadeIn()
	{
		float alpha = 1f;
		while (alpha > 0f)
		{
			alpha -= (float)GetProcessDeltaTime() / FadeTime; 
			alpha = Math.Clamp(alpha, 0f, 1f); 
			Modulate = new Color(0, 0, 0, alpha);
			await Task.Delay(1);
		}
		Modulate = new Color(0, 0, 0, 0);
	}

	public async Task FadeOut(Action callback = null)
	{
		float alpha = Modulate.A;
		while (alpha < 1f)
		{
			alpha += (float)GetProcessDeltaTime() / FadeTime; 
			alpha = Math.Clamp(alpha, 0f, 1f);
			Modulate = new Color(0, 0, 0, alpha);
			await Task.Delay(1);
		}
		Modulate = new Color(0, 0, 0, 1);
		callback?.Invoke();
	}
}
