using Godot;
using Nindot.LMS.Msbt.TagLib.Smo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonFlow.LMS.Msbt;

public partial class TagWheel : Control
{
	public Vector2I CaretPosition = Vector2I.Zero;

	private MsbtPageEditor Parent = null;
	private MouseLine MouseLine = new();
	private readonly List<TagWheelButton> Buttons = [];

	[Signal]
	public delegate void FinishedAddTagEventHandler(TagWheelTagResult tag);

	public override void _Ready()
	{
		// Get access to parent
		var parent = GetParent();
		if (parent.GetType() != typeof(MsbtPageEditor))
			throw new Exception("Invalid parent type");

		Parent = (MsbtPageEditor)parent;

		// Ensure there are buttons in the wheel
		if (GetChildCount() == 0) throw new Exception("Invalid node, use tag_wheel.tscn scene");

		// Create button list
		foreach (var child in GetChildren())
			if (child.GetType() == typeof(TagWheelButton)) Buttons.Add((TagWheelButton)child);

		// Set the focus node to the first wheel item
		Buttons[0].GrabFocus();

		// Setup wheel buttons
		var buttonCount = Buttons.Count;
		var buttonSize = Vector2.One * (100 - (buttonCount * 4.17F));
		for (int i = 0; i < buttonCount; i++)
		{
			// Set button size
			var button = Buttons[i];
			button.Size = buttonSize;

			// Move button position to position in wheel
			var pos = Vector2.Up * (buttonSize * 1.8F); // Set initial position above based on size
			pos = pos.Rotated((float)(Math.PI * 2 * (i / (float)buttonCount))); // Rotate position based on button index
			pos += -button.Size / 2; // Adjust to center position
			button.Position = pos;

			// Assign neighboring connections
			AssignNeighbors([.. Buttons], button);

			// Connect to signals
			button.AddTag += OnTagAddRequest;
		}

		// Ensure entire wheel is on screen
		ClampWithinMargin(Vector2.One * (240 - (buttonCount * 9.5F)));

		// Setup extra/generic children
		MouseLine.OriginOffset = CaretPosition - Position;
		AddChild(MouseLine);

		// Play short appear animation
		Scale = Vector2.Zero;

		var tween = CreateTween();
		tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		tween.TweenProperty(this, "scale", Vector2.One, 0.12);
	}

	public override void _Process(double delta)
	{
		// Check to see if any wheel button has focus
		foreach (var child in GetChildren())
		{
			if (child.GetType() != typeof(TagWheelButton)) continue;
			if (((TagWheelButton)child).HasFocus()) return;
		}

		// If not, close wheel
		QueueFree();
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

	// ====================================================== //
	// ==================== Signal Events =================== //
	// ====================================================== //

	void OnTagAddRequest(TagWheelTagResult request)
	{
		EmitSignal(SignalName.FinishedAddTag, [request]);
		QueueFree();
	}

	// ====================================================== //
	// ====================== Utilities ===================== //
	// ====================================================== //

	private void ClampWithinMargin(Vector2 margin)
	{
		var windowSize = GetViewport().GetWindow().Size;
		var gPos = GlobalPosition;

		if (gPos.Y > windowSize.Y - margin.Y) gPos = new(gPos.X, windowSize.Y - margin.Y);
		else if (gPos.Y < margin.Y) gPos = new(gPos.X, margin.Y);

		if (gPos.X > windowSize.X - margin.X) gPos = new(windowSize.X - margin.X, gPos.Y);
		else if (gPos.X < margin.X) gPos = new(margin.X, gPos.Y);

		GlobalPosition = gPos;
	}

	// ====================================================== //
	// =============== Focus Neighbor Utilities ============= //
	// ====================================================== //

	// Yes I'm aware this code is very ugly and slow and gross
	// I'm 100% sure there is some better fancy math way to do this
	// Too bad!
	private static void AssignNeighbors(List<TagWheelButton> buttons, TagWheelButton button)
	{
		var i = buttons.IndexOf(button);
		if (i == -1) throw new Exception("Button not in list");

		var pos = button.Position;
		var posTL = pos + (button.Size / 2);

		// FocusNext
		if (buttons.Count - 1 == i) button.FocusNext = buttons[0].GetPath();
		else button.FocusNext = buttons[i + 1].GetPath();

		// FocusPrev
		if (i == 0) button.FocusPrevious = buttons.Last().GetPath();
		else button.FocusPrevious = buttons[i - 1].GetPath();

		// FocusTop
		if (i == 0) button.FocusNeighborTop = button.GetPath();
		else if (buttons.Count - 1 == i) button.FocusNeighborTop = buttons[0].GetPath();
		else if (i == buttons.Count / 2) button.FocusNeighborTop = buttons[0].GetPath();
		else if (i < buttons.Count / 2) button.FocusNeighborTop = buttons[i - 1].GetPath();
		else if (i > buttons.Count / 2) button.FocusNeighborTop = buttons[i + 1].GetPath();

		// FocusBot
		if (i == 0) button.FocusNeighborBottom = buttons[buttons.Count / 2].GetPath();
		else if (i == buttons.Count / 2) button.FocusNeighborBottom = button.GetPath();
		else if (i < buttons.Count / 2) button.FocusNeighborBottom = buttons[i + 1].GetPath();
		else if (i > buttons.Count / 2) button.FocusNeighborBottom = buttons[i - 1].GetPath();

		// FocusLeft
		if (i == 0) button.FocusNeighborLeft = buttons.Last().GetPath();
		else if (MathUtil.AlmostZero(posTL.Y)) button.FocusNeighborLeft = buttons[buttons.Count / 4 * 3].GetPath();
		else if (posTL.Y < 0) button.FocusNeighborLeft = buttons[i - 1].GetPath();
		else if (posTL.Y > 0) button.FocusNeighborLeft = buttons[i + 1].GetPath();

		// FocusRight
		if (i == buttons.Count - 1) button.FocusNeighborRight = buttons[0].GetPath();
		else if (MathUtil.AlmostZero(posTL.Y)) button.FocusNeighborRight = buttons[buttons.Count / 4].GetPath();
		else if (posTL.Y < 0) button.FocusNeighborRight = buttons[i + 1].GetPath();
		else if (posTL.Y > 0) button.FocusNeighborRight = buttons[i - 1].GetPath();
	}
}
