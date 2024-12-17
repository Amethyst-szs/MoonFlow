using Godot;
using System;

namespace MoonFlow.Scene.EditorEvent;

[GlobalClass]
[ScenePath("res://scene/editor/event/node/port_out.tscn")]
[Icon("res://asset/material/graph/port.svg")]
public partial class PortOut : TextureRect
{
	#region Properties

	// ~~~~~~~~~~~ Node References ~~~~~~~~~~~ //

	public EventFlowNode Parent { get; private set; } = null;

	private PortIn ConnectionHover = null;

	private EventFlowNode _connection = null;
	public EventFlowNode Connection
	{
		get { return _connection; }
		private set
		{
			// If the connection is changing, disconnect from the old connection's signals
			if (value != Connection && Connection != null)
				Connection.Disconnect(EventFlowNode.SignalName.NodeMoved, Callable.From(CalcConnectionLine));

			bool isNotNull = value != null;

			_connection = value;
			ConnectionHover = null;
			ConnectionLine.Visible = isNotNull;

			// If a connection is being set, update dragger rendering and signals
			if (isNotNull)
			{
				Connection.Connect(EventFlowNode.SignalName.NodeMoved, Callable.From(CalcConnectionLine));
				CalcConnectionLine();
			}
		}
	}

	// ~~~~~~~~~~~~ Grabber State ~~~~~~~~~~~~ //

	private bool _isDrag = false;
	public bool IsDrag
	{
		get { return _isDrag; }
		private set
		{
			_isDrag = value;
			DraggerLine.Visible = value;

			if (value)
			{
				EmitSignal(SignalName.PortGrabbed);
				GrabCollider.ProcessMode = ProcessModeEnum.Inherit;
				DraggerLineCurve ??= new();
			}
			else
			{
				EmitSignal(SignalName.PortReleased);
				GrabCollider.ProcessMode = ProcessModeEnum.Disabled;
			}
		}
	}

	// ~~~~~~~~~ Internal References ~~~~~~~~~ //

	[Export]
	private Line2D ConnectionLine;
	[Export]
	private Line2D DraggerLine;
	[Export]
	private Area2D GrabCollider;

	// ~~~~~~~~~~ Dragger Rendering ~~~~~~~~~~ //

	private Curve2D DraggerLineCurve = null;
	private Vector2 DraggerLineTarget = Vector2.Zero;

	// ~~~~~~~~~~ Signal Definitions ~~~~~~~~~ //

	[Signal]
	public delegate void PortGrabbedEventHandler();
	[Signal]
	public delegate void PortReleasedEventHandler();

	#endregion

	#region Initilization

	public override void _Ready()
	{
		// Hide line rendering
		ConnectionLine.Hide();
		DraggerLine.Hide();

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

		// Connect to signals from parent
		Parent.Connect(EventFlowNode.SignalName.NodeMoved, Callable.From(CalcConnectionLine));
	}

	#endregion

	#region Process & Render

	public override void _Process(double delta)
	{
		if (!IsDrag)
			return;

		CalcLineRender();

		// Update collider
		GrabCollider.GlobalPosition = GetGlobalMousePosition();
	}

	private void CalcLineRender(bool isUpdateToMouse = true)
	{
		// Reset curve
		DraggerLineCurve.ClearPoints();

		// Setup curve points
		if (isUpdateToMouse)
			DraggerLineTarget = DraggerLineTarget.Lerp(DraggerLine.GetLocalMousePosition(), 0.1F);

		Vector2 mouse = DraggerLineTarget;
		Vector2 midpoint = mouse / 2;

		// Create curve points and apply to line renderer
		DraggerLineCurve.AddPoint(Vector2.Zero);
		DraggerLineCurve.AddPoint(midpoint, new Vector2(-midpoint.X / 2, -midpoint.Y), new Vector2(midpoint.X / 2, midpoint.Y));
		DraggerLineCurve.AddPoint(mouse);

		DraggerLine.Points = DraggerLineCurve.GetBakedPoints();

		// Update shader direction sign
		float sign = MathF.Sign(DraggerLineTarget.Y) * MathF.Sign(DraggerLineTarget.X);
		SetDraggerLineShaderDirection(sign);
	}

	private void CalcConnectionLine()
	{
		if (!IsInstanceValid(Connection))
			return;
		
		var inPos = Connection.PortIn.GlobalPosition;
		var inSize = Connection.PortIn.Size;
		DraggerLineTarget = inPos - ConnectionLine.GlobalPosition + (inSize / 2);
		CalcLineRender(false);

		ConnectionLine.Points = DraggerLine.Points;
	}

	private void SetDraggerLineShaderDirection(float sign)
	{
		var shader = DraggerLine.Material as ShaderMaterial;
		shader.SetShaderParameter("direction", sign);
	}

	#endregion

	#region Input

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
		
		// Capture all left mouse inputs regardless of press or release
		GetViewport().SetInputAsHandled();

		// If clicking on the port, enable the grabbing
		if (m.Pressed)
		{
			IsDrag = true;
			DraggerLineTarget = Vector2.Zero;
			return;
		}

		// If releasing the mouse, reset dragger state and set connection
		IsDrag = false;

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

	#endregion
}
