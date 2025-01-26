using Godot;
using System;

using Nindot.LMS.Msbt; 

namespace MoonFlow.Scene;

public partial class MsbtEntryParserExceptionInfoScene : VBoxContainer
{
	[Export]
	private Label LabelSourceFile;
	[Export]
	private Label LabelSourceEntry;

	public void AppearInfo(MsbtEntryParserException e)
	{
		LabelSourceFile.SetDeferred(Label.PropertyName.Text, e.File);
		LabelSourceEntry.SetDeferred(Label.PropertyName.Text, e.Entry);

		CallDeferred(MethodName.Show);
	}
}
