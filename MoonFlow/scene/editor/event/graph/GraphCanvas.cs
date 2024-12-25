using Godot;
using System;

using Nindot.Al.EventFlow;
using System.Collections.Generic;

namespace MoonFlow.Scene.EditorEvent;

public partial class GraphCanvas : CanvasLayer
{
    #region Initilization

    public Graph Graph { get; protected set; } = null;
	public EventFlowApp Parent { get; protected set; } = null;

    public override async void _Ready()
    {
        // Search upward for EventFlowApp parent
        Godot.Node nextParent = this;
		while (Parent == null)
		{
			nextParent = nextParent.GetParent();
			if (!IsInstanceValid(nextParent))
				throw new NullReferenceException("Node is not a child of an GraphCanvas!");

			if (nextParent.GetType() == typeof(EventFlowApp))
				Parent = nextParent as EventFlowApp;
		}

        if (!Parent.IsInitCompleted)
            await ToSignal(Parent, EventFlowApp.SignalName.FileOpenComplete);

        // Set graph reference from app
        Graph = Parent.Graph;
    }

    #endregion

    #region Input

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_copy")) UnhandledInputCopy();
        if (@event.IsActionPressed("ui_paste")) UnhandledInputPaste();
    }

    private void UnhandledInputCopy()
    {
        GetSelectedData(out List<EventFlowNodeCommon> nodes,
            out Dictionary<string, EventFlowEntryPoint> entryPoints);
        
        GraphNodeClipboardServer.Copy(nodes, entryPoints);
        GetViewport().SetInputAsHandled();
    }

    private void UnhandledInputPaste()
    {
        GraphNodeClipboardServer.Paste(this);
        GetViewport().SetInputAsHandled();
    }

    #endregion

    #region Signals

    private bool IsMouseDragSelectionActive = false;

    [Signal]
	public delegate void DeselectAllEventHandler();
    [Signal]
	public delegate void SelectAllEventHandler();
    [Signal]
	public delegate void DragSelectionEventHandler(Vector2 dist);
    [Signal]
	public delegate void ContentModifiedEventHandler();
    [Signal]
	public delegate void ToggleDebugDataViewEventHandler(bool isActive);

    private void DeselectAllNodes() { EmitSignal(SignalName.DeselectAll); }
    private void SelectAllNodes() { EmitSignal(SignalName.SelectAll); }
    public void DragSelectedNodes(Vector2 dist) {
        if (IsMouseDragSelectionActive)
            return;
        
        dist *= new Vector2(1.0F / Scale.X, 1.0F / Scale.Y);
        EmitSignal(SignalName.DragSelection, dist);
    }

    private void SetMouseDragSelectionState(bool isActive) { IsMouseDragSelectionActive = isActive; }

    public void OnNodeModified() { EmitSignal(SignalName.ContentModified); }
    
    private void SetDebugDataViewState(bool isActive) { EmitSignal(SignalName.ToggleDebugDataView, isActive); }

    #endregion

    #region Utility

    private void GetSelectedData(out List<EventFlowNodeCommon> nodes,
        out Dictionary<string, EventFlowEntryPoint> enters)
    {
        nodes = [];
        enters = [];

        foreach (var child in Parent.GraphNodeHolder.GetChildren())
        {
            var t = child.GetType();
            if (t != typeof(EventFlowNodeBase) && !t.IsSubclassOf(typeof(EventFlowNodeBase)))
                continue;
            
            var node = (EventFlowNodeBase)child;
            if (!node.IsSelected)
                continue;
            
            if (node is EventFlowEntryPoint point)
                enters.Add(node.Name, point);
            else
                nodes.Add((EventFlowNodeCommon)node);
        }
    }

    #endregion
}
