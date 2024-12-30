using Godot;

using MoonFlow.Ext;

namespace MoonFlow.Scene.EditorEvent;

[ScenePath("res://scene/editor/event/param/param_editor_float.tscn")]
public partial class ParamEditorFloat : EventNodeParamEditorBase
{
	[Export]
	private SpinBox Spin;

	public override void Init()
	{
		Spin.Prefix = Param.SplitCamelCase();

		if (!Node.Content.TryGetParam(Param, out float value))
		{
			Spin.Editable = false;
			return;
		}

		ButtonAddProperty.QueueFree();
		Spin.SetValueNoSignal(value);
	}

	public override void AddPropertyToNode()
	{
		base.AddPropertyToNode();

		Spin.Editable = true;
		SetValue(0);
	}

	private void SetValue(float value)
	{
		Node.Content.TrySetParam(Param, value);
	}
}
