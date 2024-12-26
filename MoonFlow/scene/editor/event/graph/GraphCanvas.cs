using Godot;
using System;

using Nindot.Al.EventFlow;
using System.Collections.Generic;
using MoonFlow.Project;

namespace MoonFlow.Scene.EditorEvent;

public partial class GraphCanvas : CanvasLayer
{
    #region Initilization

    public Graph Graph { get; protected set; } = null;
    public EventFlowApp Parent { get; protected set; } = null;
    public GraphNodeUndoRedoServer UndoRedoServer = null;

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

        // Setup undo/redo server
        UndoRedoServer = new(this);
    }

    #endregion

    #region Input

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton m)
        {
            if (m.ButtonIndex != MouseButton.Right || !m.Pressed)
                return;

            TryOpenInjectMenu();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (@event.IsActionPressed("ui_copy", false, true)) UnhandledInputCopy();
        if (@event.IsActionPressed("ui_paste", false, true)) UnhandledInputPaste();
        if (@event.IsActionPressed("ui_graph_delete", false, true)) UnhandledInputDelete();
        if (@event.IsActionPressed("ui_undo", false, true)) UnhandledInputUndo();
        if (@event.IsActionPressed("ui_redo", false, true)) UnhandledInputRedo();
    }

    private void UnhandledInputCopy()
    {
        GetSelectedData(out List<EventFlowNodeCommon> nodes);

        GraphNodeClipboardServer.Copy(nodes);
        GetViewport().SetInputAsHandled();
    }
    private void UnhandledInputPaste()
    {
        GraphNodeClipboardServer.Paste(this);
        GetViewport().SetInputAsHandled();
    }
    private void UnhandledInputDelete()
    {
        GetSelectedData(out List<EventFlowNodeCommon> nodes);
        foreach (var node in nodes)
            node.DeleteNode();

        GetViewport().SetInputAsHandled();
    }
    private void UnhandledInputUndo()
    {
        UndoRedoServer.Undo();
        GetViewport().SetInputAsHandled();
    }
    private void UnhandledInputRedo()
    {
        UndoRedoServer.Redo();
        GetViewport().SetInputAsHandled();
    }

    #endregion

    #region Inject Menu

    public Vector2 InjectNodePosition = Vector2.Zero;
    private Vector2 InjectScreenPosition = Vector2.Zero;

    private void TryOpenInjectMenu()
    {
        // Copy the position of the mouse pointer into the inject target
        var mouse = Parent.GetLocalMousePosition();
        InjectNodePosition = mouse - Offset;

        InjectScreenPosition = Parent.GetGlobalMousePosition();

        // Access inject menu
        var node = ProjectManager.SceneRoot.FindChild(PopupInjectGraphNode.DefaultNodeName, false, false);
        if (node is not PopupInjectGraphNode popup)
            throw new Exception("Could not access PopupInjectGraphNode!");
        
        popup.SetupWithContext(Parent);
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
    public delegate void DragSelectionEndedEventHandler();

    [Signal]
    public delegate void ContentModifiedEventHandler();
    [Signal]
    public delegate void ToggleDebugDataViewEventHandler(bool isActive);

    private void DeselectAllNodes() { EmitSignal(SignalName.DeselectAll); }
    private void SelectAllNodes() { EmitSignal(SignalName.SelectAll); }
    public void DragSelectedNodes(Vector2 dist)
    {
        if (IsMouseDragSelectionActive)
            return;

        dist *= new Vector2(1.0F / Scale.X, 1.0F / Scale.Y);
        EmitSignal(SignalName.DragSelection, dist);
    }
    public void DragSelectedNodesEnd()
    {
        EmitSignal(SignalName.DragSelectionEnded);
    }

    private void SetMouseDragSelectionState(bool isActive) { IsMouseDragSelectionActive = isActive; }

    public void OnNodeModified() { EmitSignal(SignalName.ContentModified); }

    private void SetDebugDataViewState(bool isActive) { EmitSignal(SignalName.ToggleDebugDataView, isActive); }

    #endregion

    #region Utility

    private void GetSelectedData(out List<EventFlowNodeCommon> nodes)
    {
        nodes = [];

        foreach (var child in Parent.GraphNodeHolder.GetChildren())
        {
            var t = child.GetType();
            if (t != typeof(EventFlowNodeCommon) && !t.IsSubclassOf(typeof(EventFlowNodeCommon)))
                continue;

            var node = (EventFlowNodeCommon)child;
            if (!node.IsSelected)
                continue;

            nodes.Add(node);
        }
    }

    #endregion
}
