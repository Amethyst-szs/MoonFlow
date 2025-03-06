using Godot;

namespace MoonFlow.Scene.EditorEvent;

[SceneUid("uid://1i2nc665n3st")]
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
