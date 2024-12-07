using Godot;
using Nindot.LMS.Msbt.TagLib.Smo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonFlow.LMS.Msbt;

[ScenePath("res://ninode/lms/msbt/wheel/tag_wheel.tscn")]
public partial class TagWheel : Control
{
	public Vector2I CaretPosition = Vector2I.Zero;

	private MouseLine MouseLine = new();
	private readonly List<Control> Pages = [];

	[Signal]
	public delegate void FinishedAddTagEventHandler(TagWheel wheel, TagWheelTagResult tag);
	[Signal]
	public delegate void MigrateSubmenuEventHandler(TagWheel wheel, TagSubmenuBase menu);

	public override void _Ready()
	{
		// Ensure there are buttons in the wheel
		if (GetChildCount() == 0) throw new Exception("Invalid node, use tag_wheel.tscn scene");

		SetupWheel(GetNode<Control>("%WheelP1"));
		SetupWheel(GetNode<Control>("%WheelP2"));

		// Show first wheel page
		Control firstPage = Pages[0];
		Control firstButton = firstPage.GetChild(0) as Control;

		firstPage.Show();
		firstButton.GrabFocus();

		// Ensure entire wheel is on screen
		ClampWithinMargin(Vector2.One * 160F);

		// Setup extra/generic children
		MouseLine.OriginOffset = CaretPosition - Position;
		AddChild(MouseLine);

		// Play short appear animation
		Scale = Vector2.Zero;

		var tween = CreateTween();
		tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		tween.TweenProperty(this, "scale", Vector2.One, 0.12);
	}

	public override void _Process(double _)
	{
		// Check to see if any wheel button has focus
		if (!TagSubmenuBase.AnyChildHasFocus(this))
			QueueFree();
	}

	public override void _Draw()
	{
		Vector2 size = (GetChild(0) as Control).Size;
		DrawCircle(Vector2.Zero, size.X, Color.Color8(0, 0, 0, 120));
	}

	public override void _Input(InputEvent @event)
	{
		bool isFreeWheel = false;

		switch (@event)
		{
			case InputEventMouseButton:
				var m = (InputEventMouseButton)@event;
				isFreeWheel |= m.Pressed && m.ButtonIndex == MouseButton.Right;
				break;
			case InputEventKey:
				var k = (InputEventKey)@event;
				isFreeWheel |= k.IsActionPressed("ui_add_tag", false, true);
				isFreeWheel |= k.IsActionPressed("ui_cancel", false, true);
				break;
		}

		if (isFreeWheel)
		{
			QueueFree();
			GetViewport().SetInputAsHandled();
		}
	}

	// ====================================================== //
	// ==================== Signal Events =================== //
	// ====================================================== //

	private void OnTagAddRequest(TagWheelTagResult request)
	{
		EmitSignal(SignalName.FinishedAddTag, [this, request]);
		QueueFree();
	}

	private void OnTagWheelAddSubmenu(TagSubmenuBase menu)
	{
		// Position menu in location of wheel
		menu.SetupPosition(GlobalPosition);

		EmitSignal(SignalName.MigrateSubmenu, [this, menu]);
		QueueFree();
	}

	private async void OnCyclePagePressed()
	{
		int selectedIdx = 0;
		foreach (var page in Pages)
		{
			if (page.Visible)
			{
				page.Hide();
				selectedIdx = page.GetIndex();
				break;
			}
		}

		var newPage = Pages[(selectedIdx + 1) % Pages.Count];
		newPage.Show();
		(newPage.GetChild(0) as Control).GrabFocus();

		// Spin animation
		MouseLine.Hide();
		Rotation = (float)Math.PI * 1.5F;

		var tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		tween.TweenProperty(this, "rotation", Math.PI * 2, 0.25);
		await ToSignal(tween, "finished");

		MouseLine.Show();
	}

	// ====================================================== //
	// ====================== Utilities ===================== //
	// ====================================================== //

	private void SetupWheel(Control root)
	{
		// Setup page
		Pages.Add(root);
		root.Hide();

		// Create button list
		List<TagWheelButton> buttons = [];
		foreach (var child in root.GetChildren())
			if (child.GetType() == typeof(TagWheelButton)) buttons.Add((TagWheelButton)child);

		// Setup wheel buttons
		var buttonCount = buttons.Count;
		var buttonSize = Vector2.One * (110 - (buttonCount * 4.17F));
		root.Size = buttonSize * 2.0F;

		for (int i = 0; i < buttonCount; i++)
		{
			// Set button size
			var button = buttons[i];
			button.Size = buttonSize;

			// Move button position to position in wheel
			var baseDist = Vector2.One * 215;
			var pos = Vector2.Up * (baseDist - (buttonSize * 1.5F)); // Set initial position above based on size
			pos = pos.Rotated((float)(Math.PI * 2 * (i / (float)buttonCount))); // Rotate position based on button index
			pos += -button.Size / 2; // Adjust to center position
			button.Position = pos;

			// Assign neighboring connections
			AssignNeighbors([.. buttons], button);

			// Connect to signals
			button.Connect(TagWheelButton.SignalName.AddSubmenu,
				Callable.From(new Action<TagSubmenuBase>(OnTagWheelAddSubmenu)));

			button.Connect(TagWheelButton.SignalName.AddTag,
				Callable.From(new Action<TagWheelTagResult>(OnTagAddRequest)));
		}
	}

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
