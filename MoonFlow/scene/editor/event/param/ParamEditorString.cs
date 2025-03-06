using Godot;

namespace MoonFlow.Scene.EditorEvent;

[SceneUid("uid://kxj2pc4o5t84")]
public partial class ParamEditorString : EventNodeParamEditorBase
{
	[Export]
	private Label EditHeader;
	[Export]
	private LineEdit Edit;

	public override void Init()
	{
		EditHeader.Text = Param.SplitCamelCase();

		if (!Node.Content.TryGetParam(Param, out string value))
		{
			Edit.Editable = false;
			return;
		}

		ButtonAddProperty.QueueFree();
		Edit.Text = value;
	}

	public override void AddPropertyToNode()
	{
		base.AddPropertyToNode();

		Edit.Editable = true;
		SetValue("");
	}

	private void SetValue(string value)
	{
		Node.Content.TrySetParam(Param, value);
	}
}
