using Godot;
using MoonFlow.Project;
using System;

namespace MoonFlow.Scene.Home;

public partial class SubtabConfigInfo : VBoxContainer
{
    [Export, ExportGroup("Internal References")]
    private LineEdit LineNickname;

    public override void _Ready()
    {
        LineNickname.Text = GetNickname();
    }

    #region Signals

    private static void OnTextSubmittedProjectNickname(string txt) => SetNickname(txt);
    private static async void OnDefaultLanguageChangeSelectionMade(string lang, int idx)
    {
        string defaultLang = ProjectManager.GetDefaultLang();
        if (lang == defaultLang)
            return;
        
        ProjectConfig config = ProjectManager.GetConfig();
        config.ForceChangeDefaultLanguage(lang);
        config.WriteFile();

        var isValidReload = await AppSceneServer.TryCloseAllApps();
		if (!isValidReload)
			return;

		var path = ProjectManager.GetProject().Path;
		ProjectManager.TryOpenProject(ref path, out _);
    }

    #endregion

    #region Utility

    private static string GetNickname()
    {
        ProjectState proj = ProjectManager.GetProject() ?? throw new NullReferenceException();
        return proj.Config.LocalConfig.Data.ProjectNickname;
    }
    private static void SetNickname(string nick)
    {
        ProjectState proj = ProjectManager.GetProject() ?? throw new NullReferenceException();
        ProjectLocalConfig config = proj.Config.LocalConfig;

        config.Data.ProjectNickname = nick;
        config.WriteFile();
    }

    #endregion
}
