using System;
using System.Linq;
using Godot;
using Godot.Collections;
using MoonFlow.Scene.Main;

namespace MoonFlow.Scene;

[Tool]
[GlobalClass]
public partial class WikiAccessorResource : Resource
{
    [Export(PropertyHint.File, "*.md")]
    public string Path
    {
        get { return EngineSettings.GetWiki() + LocalPath; }
        set
        {
            if (!Engine.IsEditorHint())
                return;

            var prefix = EngineSettings.GetSetting<string>("moonflow/wiki/local_source", "");
            var result = value.TrimPrefix(prefix);
            Set(PropertyName.LocalPath, result);
        }
    }

    [Export]
    public string LocalPath { get; private set; } = "";

    public void OpenWiki(SceneTree tree)
    {
        var wiki = EngineSettings.GetWiki();
        if (wiki.StartsWith("https://"))
        {
            OpenWikiRemote(wiki);
            return;
        }

        OpenWikiLocal(tree, wiki);
    }

    public void OpenWikiLocal(SceneTree tree) { OpenWikiLocal(tree, EngineSettings.GetWikiLocal()); }
    public void OpenWikiLocal(SceneTree tree, string wiki)
    {
        if (!wiki.StartsWith("res://"))
        {
            GD.PushWarning("Unrecognized path type for wiki access!");
            return;
        }

        var app = SceneCreator<AppLocalWikiViewer>.Create();

        var scene = tree.CurrentScene;
        if (scene is not MainSceneRoot sceneRoot)
        {
            GD.PushError("Cannot open documentation without scene root");
            app.QueueFree();
            return;
        }

        app.SetResource(wiki + LocalPath, LocalPath);
        sceneRoot.NodeApps.AddChild(app);

        app.SetupWikiApp();
    }

    public void OpenWikiRemote() { OpenWikiRemote(EngineSettings.GetWikiRemote()); }
    public void OpenWikiRemote(string wiki)
    {
        OS.ShellOpen(wiki + LocalPath);
    }

    #region Editor Inspector

    private static readonly string[] ReadOnlyProperties = ["WikiLocalPath"];

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