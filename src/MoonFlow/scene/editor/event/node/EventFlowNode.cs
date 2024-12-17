using Godot;
using System;

namespace MoonFlow.Scene.EditorEvent;

[GlobalClass]
[ScenePath("res://scene/editor/event/node/event_flow_node.tscn")]
public partial class EventFlowNode : Node2D
{
	#region Properties

	// ~~~~~~~~~~~ Node References ~~~~~~~~~~~ //

	public GraphCanvas Parent { get; private set; } = null;

	// ~~~~~~~~~ Internal References ~~~~~~~~~ //

	[Export, ExportGroup("Internal References")]
	public PortIn PortIn { get; private set; }
	[Export]
	private Panel SelectionPanel;

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

	private Vector2 RawPosition;
	private const float PositionSnapSize = 16.0F;

	// ~~~~~~~~~~~~~~~ Signals ~~~~~~~~~~~~~~~ //

	[Signal]
	public delegate void NodeMovedEventHandler();

    #endregion

    #region Initilization

    public override void _Ready()
    {
		// Search upward for parent graph
		Node nextParent = this;
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

		if (Position != oldPos)
			EmitSignal(SignalName.NodeMoved);
	}

	#endregion
}
