using Godot;
using System;

namespace MoonFlow.LMS.Msbt;

[ScenePath("res://ninode/lms/msbt/entry/components/msbt_entry_page_separator.tscn")]
public partial class MsbtEntryPageSeparator : HBoxContainer
{
	public int PageIndex = 0;

	[Signal]
	public delegate void AddPageEventHandler(int idx);

	private void OnAddPagePressed()
	{
		EmitSignal(SignalName.AddPage, PageIndex);
	}
}
