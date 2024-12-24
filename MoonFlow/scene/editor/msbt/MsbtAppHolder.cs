using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;

using MoonFlow.Project;

namespace MoonFlow.Scene.EditorMsbt;

[ScenePath("res://scene/editor/msbt/msbt_editor.tscn"), Icon("res://asset/app/icon/msbt.png")]
public partial class MsbtAppHolder : AppScene
{
	public MsbtEditor Editor = null;
	public Dictionary<string, SarcMsbtFile> TextFiles = [];

	public override void _Ready()
	{
		Editor = GetNode<MsbtEditor>("MsbtEditor");
	}

	public void SetupEditor(SarcMsbpFile msgProject, string lang, string archiveName, string key)
	{
		if (Editor == null)
			throw new NullReferenceException("Wait for Ready before calling SetupEditor!");

		// Get msbt for default language as fallback
		var fallbackArc = ProjectManager.GetMSBTArchives().GetArchiveByFileName(archiveName);
		if (!fallbackArc.Content.TryGetValue(key, out ArraySegment<byte> fallback))
			throw new Exception("Default language's archive missing " + key);

		// Get access to all msbt files with this key in all languages
		var txtHolder = ProjectManager.GetMSBT();
		foreach (var txtLang in txtHolder)
		{
			var archive = txtLang.Value.GetArchiveByFileName(archiveName);
			
			if (!archive.Content.ContainsKey(key))
				archive.Content.Add(key, fallback.ToArray());
			
			var msbt = archive.GetFileMSBT(key, new MsbtElementFactoryProjectSmo());
			TextFiles.Add(txtLang.Key, msbt);
		}

		// Create msbt object and open file in editor
		AppTaskbarTitle = key;
		Editor.OpenFile(msgProject, TextFiles, lang);
		return;
	}

	public static MsbtAppHolder OpenApp(string archiveName, string key)
	{
		var editor = SceneCreator<MsbtAppHolder>.Create();
		editor.SetUniqueIdentifier(archiveName + key);
		ProjectManager.SceneRoot.NodeApps.AddChild(editor);

		var msbp = ProjectManager.GetMSBP();
		var defaultLang = ProjectManager.GetProject().Config.Data.DefaultLanguage;
		editor.SetupEditor(msbp, defaultLang, archiveName, key);

		return editor;
	}

	public static MsbtAppHolder OpenAppWithSearch(string archiveName, string key, string search)
	{
		var editor = OpenApp(archiveName, key);
		editor.Editor.UpdateEntrySearch(search);
		return editor;
	}

    public override async Task SaveFile(bool isRequireFocus)
    {
        await Editor.SaveFile(isRequireFocus);
    }

    public override string GetUniqueIdentifier(string input)
	{
		return "MSBT_" + input;
	}

	private void OnSignalContentModified() { IsModified = true; }
	private void OnSignalContentNotModified() { IsModified = false; }
}