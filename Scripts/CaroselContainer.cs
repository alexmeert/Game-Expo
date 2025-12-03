using Godot;
using System;

[Tool]
public partial class CaroselContainer : Node2D
{
	[Export] public float Spacing { get; set; } = 20.0f;

	[Export] public bool WarparoundEnabled { get; set; } = false;
	[Export] public float WrapRadius { get; set; } = 300.0f;
	[Export] public float WrapHeight { get; set; } = 50.0f;
	[Export] public float WrapYOffset { get; set; } = 50.0f;

	[Export(PropertyHint.Range, "0.0,1.0")] public float OpacityStrength { get; set; } = 0.35f;
	[Export(PropertyHint.Range, "0.0,1.0")] public float ScaleStrength { get; set; } = 0.35f;
	[Export(PropertyHint.Range, "0.01,0.99")] public float ScaleMin { get; set; } = 0.1f;

	[Export] public float SmoothingSpeed { get; set; } = 5.0f;
	[Export] public int SelectedIndex { get; set; } = 0;
	[Export] public bool FollowButtonsFocus { get; set; } = false;

	[Export] public Control PositionOffsetNode { get; set; } = null;

	public override void _Process(double delta)
	{
		if (PositionOffsetNode == null || PositionOffsetNode.GetChildCount() == 0)
			return;

		int count = PositionOffsetNode.GetChildCount();

		// Clamp selected index safely
		SelectedIndex = Mathf.Clamp(SelectedIndex, 0, count - 1);

		foreach (Node child in PositionOffsetNode.GetChildren())
		{
			if (child is not Control control) continue;

			int i = control.GetIndex();
			Vector2 pos = control.Position;
			Vector2 scale = control.Scale;
			Color mod = control.Modulate;

			if (WarparoundEnabled)
			{
				int maxRange = Mathf.Max(1, count / 2);
				float t = (i - SelectedIndex) / (float)(count - 1);
				t = Mathf.Clamp(t, -1f, 1f);

				float angle = t * Mathf.Tau;
				float x = Mathf.Sin(angle) * WrapRadius;
				float y = Mathf.Cos(angle) * WrapHeight;

				Vector2 target = new Vector2(x, y - WrapYOffset) - control.Size / 2f;
				control.Position = pos.Lerp(target, (float)(delta * SmoothingSpeed));
			}
			else
			{
				float x = 0;
				if (i > 0)
				{
					var prev = PositionOffsetNode.GetChild<Control>(i - 1);
					x = prev.Position.X + prev.Size.X + Spacing;
				}

				control.Position = new Vector2(x, -control.Size.Y / 2f);
			}

			// Set pivot offset to center
			control.PivotOffset = control.Size / 2f;

			// Scale based on distance from selected
			float scaleTarget = 1f - (ScaleStrength * Mathf.Abs(i - SelectedIndex));
			scaleTarget = Mathf.Clamp(scaleTarget, ScaleMin, 1f);
			control.Scale = scale.Lerp(Vector2.One * scaleTarget, (float)(delta * SmoothingSpeed));

			// Opacity based on distance
			float aTarget = 1f - (OpacityStrength * Mathf.Abs(i - SelectedIndex));
			aTarget = Mathf.Clamp(aTarget, 0f, 1f);
			mod.A = Mathf.Lerp(mod.A, aTarget, (float)(delta * SmoothingSpeed));
			control.Modulate = mod;

			// Z index sorting (selected is always above neighbors)
			control.ZIndex = (i == SelectedIndex) ? 1 : -Mathf.Abs(i - SelectedIndex);

			// FOLLOW FOCUS detection
			if (FollowButtonsFocus && control.HasFocus())
			{
				SelectedIndex = i;
			}
		}

		// Move container to keep the selected button centered
		Vector2 menuPos = PositionOffsetNode.Position;
		if (WarparoundEnabled)
		{
			PositionOffsetNode.Position = new Vector2(
				Mathf.Lerp(menuPos.X, 0f, (float)(delta * SmoothingSpeed)),
				menuPos.Y
			);
		}
		else
		{
			var selected = PositionOffsetNode.GetChild<Control>(SelectedIndex);
			float centerX = -(selected.Position.X + selected.Size.X / 2f)
							+ GetViewportRect().Size.X / 2f;

			PositionOffsetNode.Position = new Vector2(
				Mathf.Lerp(menuPos.X, centerX, (float)(delta * SmoothingSpeed)),
				menuPos.Y
			);
		}
	}

	public void Left()
	{
		SelectedIndex--;

		if (WarparoundEnabled)
		{
			if (SelectedIndex < 0)
				SelectedIndex = PositionOffsetNode.GetChildCount() - 1;
		}
		else
		{
			SelectedIndex = Mathf.Clamp(SelectedIndex, 0, PositionOffsetNode.GetChildCount() - 1);
		}
	}

	public void Right()
	{
		SelectedIndex++;

		if (WarparoundEnabled)
		{
			if (SelectedIndex >= PositionOffsetNode.GetChildCount())
				SelectedIndex = 0;
		}
		else
		{
			SelectedIndex = Mathf.Clamp(SelectedIndex, 0, PositionOffsetNode.GetChildCount() - 1);
		}
	}
}
