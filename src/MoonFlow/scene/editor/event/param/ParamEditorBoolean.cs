using Godot;
using System;

using MoonFlow.Scene.EditorEvent;

using CSExtensions;

namespace MoonFlow.Scene.EditorEvent;

[ScenePath("res://scene/editor/event/param/param_editor_boolean.tscn")]
public partial class ParamEditorBoolean : EventNodeParamEditorBase
{
	[Export]
	private CheckBox Check;

	public override void Init()
	{
		Check.Text = Param.SplitCamelCase();
		
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
