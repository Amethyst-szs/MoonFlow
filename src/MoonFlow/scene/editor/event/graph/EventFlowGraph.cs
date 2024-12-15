using Godot;
using System;

namespace MoonFlow.Scene.EditorEvent;

public partial class EventFlowGraph : Control
{
	[Export]
	private CanvasLayer Graph = null;

	private Control Parent = null;

    public override void _Ready()
    {
        base._Ready();

		Parent = GetParent() as Control;
		Parent.VisibilityChanged += OnVisiblityChanged;
    }

    #region Signals

    private void OnVisiblityChanged()
	{
		if (!IsInstanceValid(Parent))
			return;
		
		Graph.Visible = Parent.Visible;
	}

	#endregion
}
