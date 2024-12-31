using Godot;
using System;

using Nindot;
using Nindot.LMS.Msbt.TagLib.Smo;

using MoonFlow.Project;
using MoonFlow.Ext;

namespace MoonFlow.Scene.EditorEvent;

[GlobalClass]
[ScenePath("res://scene/editor/event/node/port/port_out.tscn")]
[Icon("res://asset/material/graph/port.svg")]
public partial class PortOut : TextureRect
{
	#region Properties

	// ~~~~~~~~~~~ Node References ~~~~~~~~~~~ //

	public EventFlowNodeBase Parent { get; private set; } = null;
	public Nindot.Al.EventFlow.Node Content
	{
		get
		{
			if (Parent is EventFlowEntryPoint)
				return null;

			return (Parent as EventFlowNodeCommon).Content;
		}
	}

	private PortIn ConnectionHover = null;

	private EventFlowNodeCommon _connection = null;
	public EventFlowNodeCommon Connection
	{
		get { return _connection; }
		set
		{
			// If the connection is changing, disconnect from the old connection's signals
			if (value != Connection && Connection != null)
				Connection.Disconnect(EventFlowNodeBase.SignalName.NodeMoved, Callable.From(CalcConnectionLine));

			bool isNotNull = value != null;

			_connection = value;
			ConnectionHover = null;
			ConnectionLine.Visible = isNotNull;

			// If a connection is being set, update dragger rendering and signals
			if (isNotNull)
			{
				var call = Callable.From(CalcConnectionLine);
				if (!Connection.IsConnected(EventFlowNodeBase.SignalName.NodeMoved, call))
					Connection.Connect(EventFlowNodeBase.SignalName.NodeMoved, call);

				CalcConnectionLine();
			}

			EmitSignal(SignalName.PortConnected, this, value?.PortIn);
		}
	}

	// ~~~~~~~~~ Internal References ~~~~~~~~~ //

	[Export, ExportGroup("Internal References")]
	private Line2D ConnectionLine;
	[Export]
	private Line2D DraggerLine;
	[Export]
	private Area2D GrabCollider;

	// ~~~~~~~~~~~ Port Information ~~~~~~~~~~ //

	[Export, ExportGroup("Port")]
	public int Index { get; private set; } = int.MinValue;
	public bool IsMessagePort
	{
		get { return Parent.IsUseMessagePorts; }
	}

	// ~~~~~~~~~~~~ Grabber State ~~~~~~~~~~~~ //

	private bool _isDrag = false;

	[Export, ExportGroup("State")]
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
			}
			else
			{
				EmitSignal(SignalName.PortReleased);
				GrabCollider.ProcessMode = ProcessModeEnum.Disabled;
			}
		}
	}

	// ~~~~~~~~~~ Dragger Rendering ~~~~~~~~~~ //

	private Curve2D DraggerLineCurve = new();
	private Vector2 DraggerLineTarget = Vector2.Zero;

	private Color _portColor = Colors.White;
	public Color PortColor
	{
		get { return _portColor; }
		set
		{
			_portColor = value;
			Modulate = value;
		}
	}

	// ~~~~~~~~~~ Constant Textures ~~~~~~~~~~ //

	public static readonly Texture2D TexPortTxt = GD.Load<Texture2D>("res://asset/material/graph/port_txt.svg");

	// ~~~~~~~~~~ Signal Definitions ~~~~~~~~~ //

	[Signal]
	public delegate void PortGrabbedEventHandler();
	[Signal]
	public delegate void PortReleasedEventHandler();
	[Signal]
	public delegate void PortConnectedEventHandler(PortOut port, PortIn connection);

	#endregion

	#region Initilization

	public override void _Ready()
	{
		// Access parent node
		Parent = this.FindParentBySubclass<EventFlowNodeBase>();
		Parent.Connect(EventFlowNodeBase.SignalName.NodeMoved, Callable.From(CalcConnectionLine));

		// Hide line rendering
		ConnectionLine.Hide();
		DraggerLine.Hide();

		// Setup port index
		Index = GetIndex();
		Name = Index.ToString();

		// If this port has a message attached to it, swap out texture and setup
		if (IsMessagePort)
		{
			Texture = TexPortTxt;
			SetupPortMessage();
		}
	}

	private void SetupPortMessage(Nindot.Al.EventFlow.NodeMessageResolverData resolver = null)
	{
		// Access case's event info
		if (Content == null)
			throw new Exception("Cannot setup port message without content!");

		if (Content.CaseEventList == null)
			Content.CaseEventList ??= new();

		var list = Content.CaseEventList;
		list.TryIncreaseCaseListSize(Index + 1);

		var caseEvent = list.CaseList[Index];
		
		// If there is no message assigned to case, assign a default value
		caseEvent.MessageData ??= new("SystemMessage", "GlossarySystem", "Answer_Yes_2");

		if (resolver != null)
			caseEvent.MessageData = resolver;

		// Assign tooltip to port
		var holder = ProjectManager.GetMSBTArchives();
		SarcFile arc = holder.GetArchiveByFileName(caseEvent.MessageData.MessageArchive);
		var msbt = arc.GetFileMSBT(caseEvent.MessageData.MessageFile + ".msbt", new MsbtElementFactoryProjectSmo());

		var txt = msbt.GetEntry(caseEvent.MessageData.LabelName);

		TooltipText = txt.GetRawText(true);
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

		if (@event.GetType() == typeof(InputEventMouseMotion))
		{
			if (IsDrag)
				GetViewport().SetInputAsHandled();

			return;
		}
	}

	private void UnhandledInputMoseButton(InputEventMouseButton m)
	{
		// If middle clicked and contains a message, open msbt selector
		bool isCtrlL = m.ButtonIndex == MouseButton.Left && m.IsCommandOrControlPressed();
		if ((isCtrlL || m.ButtonIndex == MouseButton.Middle) && IsMessagePort)
		{
			OnSelectNewTextSource();
			GetViewport().SetInputAsHandled();
			return;
		}

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
		if (ConnectionHover != null)
			Connection = ConnectionHover.Parent;

		IsDrag = false;
	}

	private void OnMouseColliderFoundInPort(Area2D area)
	{
		if (!IsPortInValid(area, out PortIn port))
			return;

		ConnectionHover = port;
	}

	private void OnMouseColliderLostInPort(Area2D area)
	{
		if (!IsPortInValid(area, out _))
			return;

		ConnectionHover = null;
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

	#region Signals

	private void OnSelectNewTextSource()
	{
		var popup = SceneCreator<PopupMsbtSelectEntry>.Create();
		GetTree().CurrentScene.AddChild(popup);
		popup.Popup();

		popup.Connect(PopupMsbtSelectEntry.SignalName.ItemSelected, Callable.From(
			new Action<string, string, string>(OnNewTextSourceSelectedFromPopup)
		));
	}

	private void OnNewTextSourceSelectedFromPopup(string arc, string file, string label)
	{
		// Setup text resolver
		arc = arc.RemoveFileExtension();
		file = file.RemoveFileExtension();
		
		var resolver = new Nindot.Al.EventFlow.NodeMessageResolverData(arc, file, label);
		SetupPortMessage(resolver);

		Parent.SetNodeModified();
	}

	#endregion

	#region Utility

	public void RemoveConnection()
	{
		if (Connection != null)
			Connection = null;
	}

	#endregion
}
