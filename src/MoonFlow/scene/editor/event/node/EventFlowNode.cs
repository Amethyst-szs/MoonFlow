using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using Nindot.Al.EventFlow;

namespace MoonFlow.Scene.EditorEvent;

[GlobalClass]
[ScenePath("res://scene/editor/event/node/event_flow_node.tscn")]
public partial class EventFlowNode : Node2D
{
	#region Properties

	// ~~~~~~~~~~~ Node References ~~~~~~~~~~~ //

	public GraphCanvas Parent { get; private set; } = null;

	public EventFlowNode[] Connections = [];

	// ~~~~~~~~~~~~ Content & Type ~~~~~~~~~~~ //

	public Graph Graph { get; private set; } = null;
	public Nindot.Al.EventFlow.Node Content { get; private set; } = null;
	public NodeMetadata Metadata { get; private set; } = null;

	public enum NodeTypes
	{
		NODE,
		ENTRY_POINT,
	}

	public NodeTypes NodeType;

	// ~~~~~~~~~ Internal References ~~~~~~~~~ //

	[Export, ExportGroup("Internal References")]
	public PortIn PortIn { get; private set; }
	[Export]
	public VBoxContainer PortOutList { get; private set; }

	[Export]
	public VBoxContainer ParamHolder { get; private set; }
	[Export]
	public VBoxContainer ParamAddDropdownHolder { get; private set; }

	[Export]
	private Panel SelectionPanel;

	[Export]
	private Label DebugDataDisplay;

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

	#endregion

	#region Initilization

	public override void _Ready()
	{
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

	public void InitContent(Nindot.Al.EventFlow.Node content, Graph graph)
	{
		// Setup state
		Graph = graph;
		Content = content;
		NodeType = NodeTypes.NODE;
		Name = content.Id.ToString();

		// Setup name and type headers
		var labelType = GetNode<Label>("%Label_Type");
		labelType.Text = Tr(Content.TypeBase, "EVENT_GRAPH_NODE_TYPE");
		labelType.TooltipText = Content.TypeBase;

		var labelName = GetNode<Label>("%Label_Name");
		labelName.Text = Content.Name;

		// Initilize properties and connections
		InitParamEditor();

		for (var i = 0; i < content.GetNextIdCount(); i++)
			CreatePortOut();

		DrawDebugLabel();
	}

	public void InitContent(string entryName, Graph graph, EventFlowNode target)
	{
		Graph = graph;
		NodeType = NodeTypes.ENTRY_POINT;
		Name = entryName;
		ParamHolder.QueueFree();

		// Setup name and type headers
		var labelType = GetNode<Label>("%Label_Type");
		labelType.Text = Tr("EntryPoint", "EVENT_GRAPH_NODE_TYPE");

		var labelName = GetNode<Label>("%Label_Name");
		labelName.Text = entryName;

		// Setup ports
		PortIn.QueueFree();

		var o = CreatePortOut();
		Connections = new EventFlowNode[1];
		o.Connection = target;

		DrawDebugLabel();
	}

	public void InitContentMetadata(GraphMetadata holder, NodeMetadata data)
	{
		if (data == null)
		{
			Metadata = new();

			switch (NodeType)
			{
				case NodeTypes.NODE:
					holder.Nodes.Add(Content.Id, Metadata);
					return;
				case NodeTypes.ENTRY_POINT:
					holder.EntryPoints.Add(Name, Metadata);
					EmitSignal(SignalName.NodeMoved);
					return;
			}
		}

		Metadata = data;

		RawPosition = Metadata.Position;
		Position = Metadata.Position;
		EmitSignal(SignalName.NodeMoved);

		DrawDebugLabel();
	}

	public void SetupConnections(List<EventFlowNode> list)
	{
		Connections = new EventFlowNode[list.Count];

		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] == null)
				continue;

			var port = PortOutList.GetChild(i) as PortOut;
			port.Connection = list[i];
		}
	}

	private PortOut CreatePortOut()
	{
		var outPort = SceneCreator<PortOut>.Create();
		PortOutList.AddChild(outPort);

		outPort.Connect(PortOut.SignalName.PortConnected, Callable.From(
			new Action<PortOut, EventFlowNode>(OnConnectionChanged)
		));

		return outPort;
	}

	#endregion

	protected virtual void InitParamEditor()
	{
		var type = Content.GetSupportedParams(out Dictionary<string, Type> pList);
		if (type == Nindot.Al.EventFlow.Node.NodeOptionType.NO_OPTIONS)
		{
			ParamAddDropdownHolder.QueueFree();
			ParamHolder.QueueFree();
			return;
		}

		// Create all param editors
		foreach (var p in pList)
			EventNodeParamFactory.CreateParamEditor(this, p.Key, p.Value);
		
		// If there are no additional properties to add, remove dropdown
		if (ParamAddDropdownHolder.GetChildCount() == 0)
			ParamAddDropdownHolder.QueueFree();
	}

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

	private void OnConnectionChanged(PortOut port, EventFlowNode connection)
	{
		Connections[port.Index] = connection;

		// Update data in graph data content
		if (IsEntryPoint())
		{
			Graph.EntryPoints[Name] = connection?.Content;
			DrawDebugLabel();
			return;
		}

		if (connection == null)
		{
			Content.RemoveNextNode(port.Index);
			DrawDebugLabel();
			return;
		}

		if (!Content.TrySetNextNode(connection.Content, port.Index))
			throw new Exception("Failed to connect " + Content.Id + " to " + connection.Content.Id);

		DrawDebugLabel();
	}

	private void SetDebugVisiblity(bool isActive)
	{
		if (IsInstanceValid(DebugDataDisplay))
			DebugDataDisplay.Visible = isActive;
	}

	#endregion

	#region Utility

	public bool IsNode() { return NodeType == NodeTypes.NODE; }
	public bool IsEntryPoint() { return NodeType == NodeTypes.ENTRY_POINT; }

	private void DrawDebugLabel()
	{
		if (DebugDataDisplay == null)
			return;

		string txt = "";

		txt += AppendDebugLabel(nameof(NodeType), Enum.GetName(NodeType));
		txt += AppendDebugLabel(nameof(Position), Position);

		if (IsEntryPoint())
		{
			txt += AppendDebugLabel(nameof(Name), Name);

			if (Graph.EntryPoints.TryGetValue(Name, out Nindot.Al.EventFlow.Node target) && target != null)
				txt += AppendDebugLabel("Target", target.Id);
		}

		if (Content != null)
		{
			txt += AppendDebugLabel(nameof(Type), Content.GetType().Name);
			txt += AppendDebugLabel(nameof(Content.Id), Content.Id);

			txt += "Next: ";
			foreach (var id in Content.GetNextIds()) txt += id.ToString() + ", ";
			txt += '\n';

			txt += AppendDebugLabel(nameof(Content.TypeBase), Content.TypeBase);
			txt += AppendDebugLabel(nameof(Content.Name), Content.Name);
		}

		DebugDataDisplay.Text = txt;
	}
	private static string AppendDebugLabel(string property, object value)
	{
		return property + ": " + value.ToString() + "\n";
	}

	#endregion
}
