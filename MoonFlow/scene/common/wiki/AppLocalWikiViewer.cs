using Godot;
using System;
using System.Linq;

namespace MoonFlow.Scene;

[ScenePath("res://scene/common/wiki/app_local_wiki_viewer.tscn")]
[Icon("res://asset/app/icon/wiki.png")]
public partial class AppLocalWikiViewer : AppScene
{
	public string FilePath { get; private set; } = "";

	[Export, ExportGroup("Internal References")]
	private RichTextLabel Markdown;
	[Export]
	private Tree Browser;

	public void SetupWikiApp() { Browser.Call("set_selection", FilePath); }

    public override string GetUniqueIdentifier(string input) { return FilePath + input; }

	public void SetResource(string resPath, string localPath)
	{
		AppTaskbarTitle = localPath.Split(['/', '\\']).Last();
		FilePath = resPath;
		SetUniqueIdentifier(resPath);
	}

	private void OnFileSelected(string path, bool isUpdateTree)
	{
		var local = EngineSettings.GetSetting<string>("moonflow/wiki/local_source", "res://docs/");
		SetResource(path, path.TrimPrefix(local));

		if (isUpdateTree)
			Browser.Call("set_selection", FilePath);
		else
			Markdown.Call("setup_app", path);
	}

	private void OnOpenRemoteDocument()
	{
		var local = EngineSettings.GetSetting<string>("moonflow/wiki/local_source", "res://docs/");
		var remote = EngineSettings.GetSetting<string>("moonflow/wiki/remote_source", 
			"https://github.com/Amethyst-szs/MoonFlow/tree/stable/MoonFlow/docs/");
		
		OS.ShellOpen(remote + FilePath.TrimPrefix(local));
	}
}
