using Godot;

namespace MoonFlow.Scene.EditorMsbt;

[SceneUid("uid://bu1wg5m8oninc")]
public partial class MsbtEntryPageSeparator : HBoxContainer
{
	public int PageIndex = 0;

	[Signal]
	public delegate void AddPageEventHandler(int idx);

	private void OnAddPagePressed()
	{
		EmitSignal(SignalName.AddPage, PageIndex);
	}

	public void UpdateAddButtonState(bool isDisableSync, bool isDefaultLang)
	{
		GetNode<Button>("%AddPage").Disabled = !isDisableSync && !isDefaultLang;
	}
}
