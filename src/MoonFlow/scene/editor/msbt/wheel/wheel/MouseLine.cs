using Godot;
using System;

namespace MoonFlow.LMS.Msbt;

[GlobalClass]
public partial class MouseLine : Line2D
{
	public Vector2 OriginOffset = Vector2.Zero;

	public override void _Ready()
	{
		Points = [OriginOffset, Vector2.Zero];
		Width = 5.0F;
		WidthCurve = GD.Load<Curve>("res://scene/editor/msbt/wheel/wheel/mouse_line_curve.tres");
		DefaultColor = new Color(1, 1, 1, 0.33F);
		BeginCapMode = LineCapMode.Round;
		EndCapMode = LineCapMode.Round;
	}

	public override void _Process(double delta)
	{
		SetPointPosition(1, GetLocalMousePosition());
	}
}
