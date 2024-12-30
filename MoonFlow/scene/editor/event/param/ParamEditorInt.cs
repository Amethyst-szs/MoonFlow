using Godot;

using MoonFlow.Ext;

namespace MoonFlow.Scene.EditorEvent;

[ScenePath("res://scene/editor/event/param/param_editor_int.tscn")]
public partial class ParamEditorInt : EventNodeParamEditorBase
{
	[Export]
	private SpinBox Spin;

	public override void Init()
	{
		Spin.Prefix = Param.SplitCamelCase();

		if (!Node.Content.TryGetParam(Param, out int value))
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

	private void SetValue(int value)
	{
		Node.Content.TrySetParam(Param, value);
	}
}
