using Godot;
using System;

namespace MoonFlow.Scene.EditorEvent;

public partial class GraphCanvas : CanvasLayer
{
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
}
