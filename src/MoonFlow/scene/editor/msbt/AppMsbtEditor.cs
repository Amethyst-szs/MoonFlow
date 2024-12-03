using Godot;
using System;

using Nindot;
using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;

using MoonFlow.LMS.Msbt;
using MoonFlow.Project;
using Nindot.LMS.Msbt.TagLib.Smo;
using System.Collections.Generic;

namespace MoonFlow.Scene;

[ScenePath("res://scene/editor/msbt/app_msbt_editor.tscn")]
public partial class AppMsbtEditor : AppScene
{
	public MsbtEditor Editor = null;
	public Dictionary<string, SarcMsbtFile> TextFiles = [];

	public override void _Ready()
	{
		Editor = GetNode<MsbtEditor>("Content");
	}

	public void SetupEditor(SarcMsbpFile msgProject, string lang, string archiveName, string key)
	{
		if (Editor == null)
			throw new NullReferenceException("Wait for Ready before calling SetupEditor!");

		// Get access to all msbt files with this key in all languages
		var txtHolder = ProjectManager.GetMSBT();
		foreach (var txt in txtHolder)
		{
			var archive = txt.Value.GetArchiveByFileName(archiveName);
			var msbt = archive.GetFileMSBT(key, new MsbtElementFactoryProjectSmo());
			TextFiles.Add(txt.Key, msbt);
		}

		// Create msbt object and open file in editor
		AppTaskbarTitle = key;
		Editor.OpenFile(msgProject, TextFiles, lang);
		return;
	}
}
