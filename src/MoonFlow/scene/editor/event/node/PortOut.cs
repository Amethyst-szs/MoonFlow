using Godot;
using System;

namespace MoonFlow.Scene.EditorEvent;

[GlobalClass]
[ScenePath("res://scene/editor/event/node/port_out.tscn")]
[Icon("res://asset/material/graph/port.svg")]
public partial class PortOut : TextureRect
{
	private bool _isGrabbed = false;
	public bool IsGrabbed
	{
		get { return _isGrabbed; }
		private set
		{
			_isGrabbed = value;
			ConnectionLine.Visible = value || Connection != null;

			if (value)
			{
				EmitSignal(SignalName.PortGrabbed);
				GrabCollider.ProcessMode = ProcessModeEnum.Inherit;
			}
			else
			{
				EmitSignal(SignalName.PortReleased);
				GrabCollider.ProcessMode = ProcessModeEnum.Disabled;
			}
		}
	}

	public EventFlowNode Parent { get; private set; } = null;

	private EventFlowNode _connection = null;
	public EventFlowNode Connection
	{
		get { return _connection; }
		private set
		{
			bool isNotNull = value != null;

			_connection = value;
			ConnectionHover = null;
			ConnectionLine.Visible = isNotNull;

			if (isNotNull)
			{
				var inPos = Connection.PortIn.GlobalPosition;
				var inSize = Connection.PortIn.Size;
				ConnectionLineTarget = inPos - ConnectionLine.GlobalPosition + (inSize / 2);
				
				CalcLineRender(false);
			}
		}
	}

	private PortIn ConnectionHover = null;

	[Export]
	private Line2D ConnectionLine;
	private Curve2D ConnectionLineCurve = new();
	private Vector2 ConnectionLineTarget = Vector2.Zero;

	[Export]
	private Area2D GrabCollider;

	[Signal]
	public delegate void PortGrabbedEventHandler();
	[Signal]
	public delegate void PortReleasedEventHandler();

	public override void _Ready()
	{
		// Hide line rendering
		ConnectionLine.Hide();

		// Search upward for parent flow node
		Node nextParent = this;
		while (Parent == null)
		{
			nextParent = nextParent.GetParent();
			if (!IsInstanceValid(nextParent))
				throw new NullReferenceException("Port is not a child of an EventFlowNode!");

			if (nextParent.GetType() == typeof(EventFlowNode))
				Parent = nextParent as EventFlowNode;
		}
	}

	public override void _Process(double delta)
	{
		if (IsGrabbed)
			CalcLineRender();
	}

	private void CalcLineRender(bool isUpdateToMouse = true)
	{
		// Reset curve
		ConnectionLineCurve.ClearPoints();

		// Setup curve points
		if (isUpdateToMouse)
			ConnectionLineTarget = ConnectionLineTarget.Lerp(ConnectionLine.GetLocalMousePosition(), 0.1F);

		Vector2 mouse = ConnectionLineTarget;
		Vector2 midpoint = mouse / 2;

		// Create curve points and apply to line renderer
		ConnectionLineCurve.AddPoint(Vector2.Zero);
		ConnectionLineCurve.AddPoint(midpoint, new Vector2(-midpoint.X / 2, -midpoint.Y), new Vector2(midpoint.X / 2, midpoint.Y));
		ConnectionLineCurve.AddPoint(mouse);

		ConnectionLine.Points = ConnectionLineCurve.GetBakedPoints();

		// Update collider
		GrabCollider.GlobalPosition = GetGlobalMousePosition();
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event.GetType() == typeof(InputEventMouseButton))
		{
			UnhandledInputMoseButton(@event as InputEventMouseButton);
			return;
		}
	}

	private void UnhandledInputMoseButton(InputEventMouseButton m)
	{
		// If right clicked, clear the current connection
		if (m.ButtonIndex == MouseButton.Right)
		{
			Connection = null;
			GetViewport().SetInputAsHandled();
			return;
		}

		if (m.ButtonIndex != MouseButton.Left)
			return;

		// If clicking on the port, enable the grabbing
		if (m.Pressed)
		{
			IsGrabbed = true;
			ConnectionLine.Show();
			ConnectionLineTarget = Vector2.Zero;
			GetViewport().SetInputAsHandled();
			return;
		}

		// If releasing the mouse, reset port
		IsGrabbed = false;
		GetViewport().SetInputAsHandled();

		// If there is an in-port hovered, set connection to the new port
		if (ConnectionHover != null)
			Connection = ConnectionHover.Parent;
	}

	private void OnMouseColliderFoundInPort(Area2D area)
	{
		if (!IsPortInValid(area, out PortIn port))
			return;

		ConnectionHover = port;
	}

	private void OnMouseColliderLostInPort(Area2D area)
	{
		if (!IsPortInValid(area, out PortIn port))
			return;

		ConnectionHover = port;
	}

	private bool IsPortInValid(Area2D area, out PortIn port)
	{
		var portRaw = area.GetParent();
		if (portRaw.GetType() != typeof(PortIn))
		{
			GD.PushWarning(area.Name + " is not PortIn type!");
			port = null;
			return false;
		}

		port = portRaw as PortIn;
		return port.Parent != Parent;
	}
}
