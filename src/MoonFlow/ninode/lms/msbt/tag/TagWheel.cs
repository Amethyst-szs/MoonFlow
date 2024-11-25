using Godot;
using Nindot.LMS.Msbt.TagLib.Smo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonFlow.LMS.Msbt;

public partial class TagWheel : Control
{
	private static readonly Vector2 ButtonSize = new(50, 50);

	public Vector2I CaretPosition = Vector2I.Zero;

	private MsbtPageEditor Parent = null;
	private MouseLine MouseLine = new();

	public override void _Ready()
	{
		// Get access to parent
		var parent = GetParent();
		if (parent.GetType() != typeof(MsbtPageEditor))
			throw new Exception("Invalid parent type");

		Parent = (MsbtPageEditor)parent;

		// Create list of buttons to add to wheel
		List<WheelButton> Buttons = [
			WheelButton.Create(Parent, new MsbtTagElementSystemColor(), "Set text color"),
			WheelButton.Create(Parent, new MsbtTagElementSystemColor(), "Set text color"),
			WheelButton.Create(Parent, new MsbtTagElementSystemColor(), "Set text color"),
			WheelButton.Create(Parent, new MsbtTagElementSystemColor(), "Set text color"),
			WheelButton.Create(Parent, new MsbtTagElementSystemColor(), "Set text color"),
			WheelButton.Create(Parent, new MsbtTagElementSystemColor(), "Set text color"),
			WheelButton.Create(Parent, new MsbtTagElementSystemColor(), "Set text color"),
			WheelButton.Create(Parent, new MsbtTagElementSystemColor(), "Set text color"),
			WheelButton.Create(Parent, new MsbtTagElementSystemColor(), "Set text color"),
			WheelButton.Create(Parent, new MsbtTagElementSystemColor(), "Set text color"),
			WheelButton.Create(Parent, new MsbtTagElementSystemColor(), "Set text color"),
			WheelButton.Create(Parent, new MsbtTagElementSystemColor(), "Set text color"),
		];

		// If wheel is positioned too close to edge of screen, move inward
		var windowSize = GetViewport().GetWindow().Size;
		var gPos = GlobalPosition;
		var edgeMargin = new Vector2(125, 125);

		if (gPos.Y > windowSize.Y - edgeMargin.Y) gPos = new(gPos.X, windowSize.Y - edgeMargin.Y);
		else if (gPos.Y < edgeMargin.Y) gPos = new(gPos.X, edgeMargin.Y);

		if (gPos.X > windowSize.X - edgeMargin.X) gPos = new(windowSize.X - edgeMargin.X, gPos.Y);
		else if (gPos.X < edgeMargin.X) gPos = new(edgeMargin.X, gPos.Y);

		GlobalPosition = gPos;

		// Add children to tree
		foreach (var button in Buttons)
			AddChild(button);

		MouseLine.OriginOffset = CaretPosition - Position;
		AddChild(MouseLine);

		// Setup wheel button positions, sizes, and focus connections
		var buttonCount = Buttons.Count;
		for (int i = 0; i < buttonCount; i++)
		{
			var button = Buttons[i];

			// Set button size and center
			button.Size = ButtonSize;

			// Move button position to position in wheel
			var pos = Vector2.Up * (ButtonSize * 1.8F); // Set initial position above based on size
			pos = pos.Rotated((float)(Math.PI * 2 * (i / (float)buttonCount))); // Rotate position based on button index
			pos += -button.Size / 2; // Adjust to center position

			button.Position = pos;

			// If first button in list, grab focus
			if (i == 0) button.GrabFocus();

			// Setup connections between buttons
			var posTL = pos + (button.Size / 2);

			// Yes I'm aware this code is very ugly and slow and gross
			// I'm 100% sure there is some better fancy math way to do this
			// Too bad!

			// FocusNext
			if (Buttons.Count - 1 == i) button.FocusNext = Buttons[0].GetPath();
			else button.FocusNext = Buttons[i + 1].GetPath();

			// FocusPrev
			if (i == 0) button.FocusPrevious = Buttons.Last().GetPath();
			else button.FocusPrevious = Buttons[i - 1].GetPath();

			// FocusTop
			if (i == 0) button.FocusNeighborTop = button.GetPath();
			else if (Buttons.Count - 1 == i) button.FocusNeighborTop = Buttons[0].GetPath();
			else if (i == Buttons.Count / 2) button.FocusNeighborTop = Buttons[0].GetPath();
			else if (i < Buttons.Count / 2) button.FocusNeighborTop = Buttons[i - 1].GetPath();
			else if (i > Buttons.Count / 2) button.FocusNeighborTop = Buttons[i + 1].GetPath();

			// FocusBot
			if (i == 0) button.FocusNeighborBottom = Buttons[Buttons.Count / 2].GetPath();
			else if (i == Buttons.Count / 2) button.FocusNeighborBottom = button.GetPath();
			else if (i < Buttons.Count / 2) button.FocusNeighborBottom = Buttons[i + 1].GetPath();
			else if (i > Buttons.Count / 2) button.FocusNeighborBottom = Buttons[i - 1].GetPath();

			// FocusLeft
			if (i == 0) button.FocusNeighborLeft = Buttons.Last().GetPath();
			else if (MathUtil.AlmostZero(posTL.Y)) button.FocusNeighborLeft = Buttons[Buttons.Count / 4 * 3].GetPath();
			else if (posTL.Y < 0) button.FocusNeighborLeft = Buttons[i - 1].GetPath();
			else if (posTL.Y > 0) button.FocusNeighborLeft = Buttons[i + 1].GetPath();

			// FocusRight
			if (i == Buttons.Count - 1) button.FocusNeighborRight = Buttons[0].GetPath();
			else if (MathUtil.AlmostZero(posTL.Y)) button.FocusNeighborRight = Buttons[Buttons.Count / 4].GetPath();
			else if (posTL.Y < 0) button.FocusNeighborRight = Buttons[i + 1].GetPath();
			else if (posTL.Y > 0) button.FocusNeighborRight = Buttons[i - 1].GetPath();
		}

		// Play short appear animation
		Scale = Vector2.Zero;

		var tween = CreateTween();
		tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		tween.TweenProperty(this, "scale", Vector2.One, 0.15);
	}

	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.GetType() != typeof(InputEventMouseButton))
			return;

		var m = (InputEventMouseButton)@event;
		if (m.Pressed && m.ButtonIndex == MouseButton.Right)
		{
			QueueFree();
			GetViewport().SetInputAsHandled();
		}
	}
}
