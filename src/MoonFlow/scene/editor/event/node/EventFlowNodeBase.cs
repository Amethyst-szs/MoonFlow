using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using Nindot.Al.EventFlow;

namespace MoonFlow.Scene.EditorEvent;

public partial class EventFlowNodeBase : Node2D
{
	#region Properties

	// ~~~~~~~~~~~ Node References ~~~~~~~~~~~ //

	public GraphCanvas Parent { get; protected set; } = null;

	// ~~~~~~~~~~~~~~~~~ Data ~~~~~~~~~~~~~~~~ //

	public Graph Graph { get; protected set; } = null;
	public NodeMetadata Metadata { get; protected set; } = null;

	// ~~~~~~~~~ Internal References ~~~~~~~~~ //

	[Export, ExportGroup("Internal References")]
	public PortIn PortIn { get; private set; }
	[Export]
	public VBoxContainer PortOutList { get; private set; }

	[Export]
	protected Panel SelectionPanel { get; private set; }

	[Export]
	protected Label DebugDataDisplay { get; private set; }

	// ~~~~~~~~~~~~~~ Selection ~~~~~~~~~~~~~~ //

	private bool _isSelected = false;

	[Export, ExportGroup("State")]
	public bool IsSelected
	{
		get { return _isSelected; }
		private set
		{
			if (value == _isSelected)
				return;

			_isSelected = value;
			SelectionPanel.Visible = value;
			GD.Print(Name + " ", value ? "Selected" : "Deselected");
		}
	}

	public Vector2 RawPosition;
	private const float PositionSnapSize = 16.0F;

	// ~~~~~~~~~~~~~~~ Signals ~~~~~~~~~~~~~~~ //

	[Signal]
	public delegate void NodeMovedEventHandler();

	public virtual bool IsNode() { return false; }
	public virtual bool IsEntryPoint() { return false; }

	#endregion

	#region Initilization

	public override void _Ready()
	{
		// Check type to ensure base isn't being initilized
		// Godot doesn't properly support abstract classes so this has to be a runtime check
		if (GetType() == typeof(EventFlowNodeBase))
			throw new Exception("Base class should not be constructed!");
		
		// Search upward for parent graph
		Godot.Node nextParent = this;
		while (Parent == null)
		{
			nextParent = nextParent.GetParent();
			if (!IsInstanceValid(nextParent))
				throw new NullReferenceException("Node is not a child of an GraphCanvas!");

			if (nextParent.GetType() == typeof(GraphCanvas))
				Parent = nextParent as GraphCanvas;
		}

		// Connect to signals from graph
		Parent.Connect(GraphCanvas.SignalName.DeselectAll, Callable.From(OnNodeDeselected));
		Parent.Connect(GraphCanvas.SignalName.SelectAll, Callable.From(OnNodeSelected));
		Parent.Connect(GraphCanvas.SignalName.DragSelection, Callable.From(new Action<Vector2>(OnNodeDragged)));

		// Setup node position
		RawPosition = new Vector2(
			MathF.Floor(Position.X / PositionSnapSize) * PositionSnapSize,
			MathF.Floor(Position.Y / PositionSnapSize) * PositionSnapSize
		);

		Position = RawPosition;

		// Hide selection panel
		SelectionPanel.Hide();

		// Setup debug panel
		DebugDataDisplay.Hide();

		if (OS.IsDebugBuild())
			Parent.Connect(GraphCanvas.SignalName.ToggleDebugDataView, Callable.From(new Action<bool>(SetDebugVisiblity)));
		else
		{
			var root = DebugDataDisplay as Godot.Node;
			while (root.GetType() != typeof(Control))
				root = root.GetParent();

			root.QueueFree();
			DebugDataDisplay = null;
		}
	}

	public virtual void InitContent(Nindot.Al.EventFlow.Node content, Graph graph) {}
	public virtual void InitContent(string entryName, Graph graph, EventFlowNodeCommon target)  {}
	public virtual void SetupConnections(List<EventFlowNodeCommon> list)  {}
	protected virtual void InitParamEditor() {}

	public virtual bool InitContentMetadata(GraphMetadata holder, NodeMetadata data)
	{
		if (data == null)
		{
			Metadata = new();
			return false;
		}

		Metadata = data;

		RawPosition = Metadata.Position;
		Position = Metadata.Position;
		EmitSignal(SignalName.NodeMoved);
		DrawDebugLabel();

		return true;
	}

	protected PortOut CreatePortOut()
	{
		var outPort = SceneCreator<PortOut>.Create();
		PortOutList.AddChild(outPort);

		outPort.Connect(PortOut.SignalName.PortConnected, Callable.From(
			new Action<PortOut, EventFlowNodeCommon>(OnConnectionChanged)
		));

		return outPort;
	}

	#endregion

	#region Selection

	private void OnNodeSelected() { OnNodeSelected(true); }
	private void OnNodeSelected(bool isMultiselect)
	{
		if (IsSelected) return;

		if (!isMultiselect)
			Parent.EmitSignal(GraphCanvas.SignalName.DeselectAll);

		IsSelected = true;
	}

	private void OnNodeDeselected()
	{
		if (!IsSelected) return;
		IsSelected = false;
	}

	private void OnNodeColliderDragged(Vector2 dist)
	{
		Parent.DragSelectedNodes(dist);
	}

	private void OnNodeDragged(Vector2 dist)
	{
		if (!IsSelected) return;

		Vector2 oldPos = Position;
		RawPosition += dist;

		Vector2 snapPos;
		snapPos.X = MathF.Floor(RawPosition.X / PositionSnapSize) * PositionSnapSize;
		snapPos.Y = MathF.Floor(RawPosition.Y / PositionSnapSize) * PositionSnapSize;
		Position = snapPos;

		Metadata.Position = snapPos;

		if (snapPos != oldPos)
			EmitSignal(SignalName.NodeMoved);

		DrawDebugLabel();
	}

	private async void OnNodeResized()
	{
		await ToSignal(Engine.GetMainLoop(), "process_frame");
		EmitSignal(SignalName.NodeMoved);
	}

	#endregion

	#region Signals

	protected virtual void OnConnectionChanged(PortOut port, EventFlowNodeCommon connection) {}
	protected void SetDebugVisiblity(bool isActive)
	{
		if (IsInstanceValid(DebugDataDisplay))
			DebugDataDisplay.Visible = isActive;
	}

	#endregion

	#region Utility

	protected virtual void DrawDebugLabel() {}
	protected static string AppendDebugLabel(string property, object value)
	{
		return property + ": " + value.ToString() + "\n";
	}

	#endregion
}
