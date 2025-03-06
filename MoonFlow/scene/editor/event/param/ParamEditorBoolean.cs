using Godot;

namespace MoonFlow.Scene.EditorEvent;

[SceneUid("uid://drhi47ys0w5hv")]
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
