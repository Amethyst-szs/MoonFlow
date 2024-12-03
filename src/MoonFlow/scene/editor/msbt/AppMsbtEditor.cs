using Godot;
using System;

using Nindot.LMS.Msbp;

using MoonFlow.LMS.Msbt;
using Nindot.LMS.Msbt;

namespace MoonFlow.Scene;

[ScenePath("res://scene/editor/msbt/app_msbt_editor.tscn")]
public partial class AppMsbtEditor : AppScene
{
	public MsbtEditor Editor = null;

	public override void _Ready()
	{
		Editor = GetNode<MsbtEditor>("Content");
	}

	public void SetupEditor(MsbpFile project, MsbtFile msbt)
	{
		if (Editor == null)
			throw new NullReferenceException("Wait for Ready before calling SetupEditor!");
		
		AppTaskbarTitle = msbt.Name;
		Editor.OpenFile(msbt, project);
	}
}
