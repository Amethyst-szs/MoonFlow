using Godot;
using System;

namespace MoonFlow.LMS.Msbt;

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
