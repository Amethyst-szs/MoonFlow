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

	public void SetupWikiApp(string resPath, string localPath)
	{
		OnResourceChanged(resPath, localPath);

		Markdown.Call("setup_app", resPath);
		Markdown.Connect("resource_changed", Callable.From(
			new Action<string, string>(OnResourceChanged)
		));
	}

    public override string GetUniqueIdentifier(string input)
    {
        return FilePath + input;
    }

	private void OnResourceChanged(string resPath, string localPath)
	{
		AppTaskbarTitle = localPath.Split(['/', '\\']).Last();
		FilePath = resPath;
		SetUniqueIdentifier(resPath);
	}

	private void OnFileSelectedFromBrowser(string path)
	{
		var local = EngineSettings.GetSetting<string>("moonflow/wiki/local_source", "res://docs/");
		OnResourceChanged(path, path.TrimPrefix(local));
		Markdown.Call("setup_app", path);
	}
}
