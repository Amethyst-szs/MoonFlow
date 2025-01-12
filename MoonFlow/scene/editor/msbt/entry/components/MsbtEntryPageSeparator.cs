using Godot;

namespace MoonFlow.Scene.EditorMsbt;

[ScenePath("res://scene/editor/msbt/entry/components/msbt_entry_page_separator.tscn")]
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
