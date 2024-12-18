using Godot;
using MoonFlow.Scene.EditorEvent;
using System;

namespace MoonFlow.Scene.EditorEvent;

[ScenePath("res://scene/editor/event/param/param_editor_boolean.tscn")]
public partial class ParamEditorBoolean : EventNodeParamEditorBase
{
	[Export]
	private CheckBox Check;

	public override void Init()
	{
		Check.Text = Param;
		
		if (!Node.Content.TryGetParam(Param, out bool value))
		{
			Check.Disabled = true;
			return;
		}

		ButtonAddProperty.QueueFree();
		Check.SetPressedNoSignal(value);
	}

    public override void AddPropertyToNode()
    {
		base.AddPropertyToNode();
		
		Check.Disabled = false;
        SetValue(false);
    }

    private void SetValue(bool state)
	{
		Node.Content.TrySetParam(Param, state);
	}
}
