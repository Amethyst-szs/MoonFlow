using Godot;
using System;
using System.Collections.Generic;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;

using MoonFlow.Project;

namespace MoonFlow.Scene.EditorMsbt;

[ScenePath("res://scene/editor/msbt/msbt_editor.tscn")]
public partial class MsbtAppHolder : AppScene
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

	public override void AppClose(bool isEndExclusive = false)
	{
		if (!Editor.IsModified)
		{
			base.AppClose(isEndExclusive);
			return;
		}

		var dialog = GetNode<ConfirmationDialog>("%Dialog_UnsavedChanges");
		dialog.Popup();
	}

	public override bool TryCloseFromTreeQuit(out SignalAwaiter awaiter)
	{
		if (!Editor.IsModified)
		{
			awaiter = null;
			AppClose();
			return true;
		}

		var dialog = GetNode<ConfirmationDialog>("%Dialog_UnsavedChanges");
		dialog.Popup();

		awaiter = ToSignal(dialog, ConfirmationDialog.SignalName.TreeExited);
		return false;
	}

	private void AppCloseDespiteUnsavedChanged()
	{
		Editor.ForceResetModifiedFlag();
		AppClose();
	}
}
