using Godot;
using System;

namespace MoonFlow.Scene.EditorEvent;

[ScenePath("res://scene/editor/event/event_flow_app.tscn")]
public partial class EventFlowApp : AppScene
{
    #region Properties

    // ~~~~~~~~~ Internal References ~~~~~~~~~ //

    [Export, ExportGroup("Internal References")]
	private GraphCanvas Graph = null;
    [Export]
	private CanvasLayer Background = null;

    #endregion

    public override void _Ready()
    {
		VisibilityChanged += OnVisiblityChanged;
    }

    private void OnVisiblityChanged()
	{
		Graph.Visible = Visible;
        Background.Visible = Visible;
	}
}
