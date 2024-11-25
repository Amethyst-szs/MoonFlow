using Godot;
using Nindot.LMS.Msbt.TagLib.Smo;
using System;
using System.Collections.Generic;

namespace MoonFlow.LMS.Msbt;

public partial class TagWheel : Control
{
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
			button.Size = new Vector2(50, 50);

			// Move button position to position in wheel
			var pos = Vector2.Up * (button.Size * 2); // Set initial position above based on size
			pos = pos.Rotated((float)(Math.PI * 2 * (i / (float)buttonCount))); // Rotate position based on button index
			pos += -button.Size / 2; // Adjust to center position0

			button.Position = pos;
		}
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
