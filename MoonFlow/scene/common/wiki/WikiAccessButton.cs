using Godot;
using Godot.Collections;
using MoonFlow.Scene.Main;
using System;
using System.Linq;

namespace MoonFlow.Scene;

[GlobalClass]
[Tool]
public partial class WikiAccessButton : Button
{
	[Export(PropertyHint.File, "*.md")]
	public string WikiPath
	{
		get { return EngineSettings.GetWiki() + WikiLocalPath; }
		set
		{
			if (!Engine.IsEditorHint())
				return;
			
			var prefix = EngineSettings.GetSetting<string>("moonflow/wiki/local_source", "");
			var result = value.TrimPrefix(prefix);
			Set(PropertyName.WikiLocalPath, result);
		}
	}

	[Export]
	private string WikiLocalPath = "";

	private string TooltipTextBase = "";

    public override void _Ready()
    {
		if (Engine.IsEditorHint())
			return;
		
		TooltipTextBase = TooltipText;

		SetupTooltipText();
		VisibilityChanged += SetupTooltipText;
    }

    public override void _Pressed()
    {
        var wiki = EngineSettings.GetWiki();
		if (wiki.StartsWith("https://"))
		{
			OS.ShellOpen(wiki + WikiLocalPath);
			return;
		}

		if (!wiki.StartsWith("res://"))
		{
			GD.PushWarning("Unrecognized path type for wiki access!");
			return;
		}

		var app = SceneCreator<AppLocalWikiViewer>.Create();

		var scene = GetTree().CurrentScene;
		if (scene is not MainSceneRoot sceneRoot)
		{
			GD.PushError("Cannot open documentation without scene root");
			app.QueueFree();
			return;
		}

		app.SetResource(wiki + WikiLocalPath, WikiLocalPath);
		sceneRoot.NodeApps.AddChild(app);
		
		app.SetupWikiApp();
    }

    #region Utility

    private void SetupTooltipText()
	{
		const string context = "WIKI_BUTTON_TOOLTIP";
		var pathBase = EngineSettings.GetWiki();

		string notice = "";
		string pathPrefix;

		if (pathBase.StartsWith("https://"))
			pathPrefix = Tr("UrlNotice", context);
		else if (pathBase.StartsWith("res://"))
			pathPrefix = Tr("LocalNotice", context);
		else
			pathPrefix = "";
		
		if (EngineSettings.GetSetting<bool>("moonflow/wiki/is_display_toggle_notice", true))
			notice = Tr("SettingNotice", context);

		TooltipText = string.Format("{0}\n{1}: {2}\n{3}",
			Tr(TooltipTextBase),
			pathPrefix,
			WikiLocalPath.TrimSuffix(".md").ToPascalCase(),
			notice
		);
	}

	#endregion

    #region Editor Inspector

    private static readonly string[] ReadOnlyProperties = [
		"WikiLocalPath",
	];

    public override void _ValidateProperty(Dictionary property)
    {
		if (ReadOnlyProperties.Contains(property["name"].ToString()))
        {
            var usage = property["usage"].As<PropertyUsageFlags>() | PropertyUsageFlags.ReadOnly;
            property["usage"] = (int)usage;
        }
    }

    #endregion
}
