using Godot;
using MoonFlow.Project;
using System;

namespace MoonFlow.Scene.Home;

public partial class SubtabMoonflowProject : MarginContainer
{
    [Export, ExportGroup("Internal References")]
    private LineEdit LineNickname;

    public override void _Ready()
    {
        LineNickname.Text = GetNickname();
    }

    #region Signals

    private void OnTextSubmittedProjectNickname(string txt) => SetNickname(txt);

    #endregion

    #region Utility

    private string GetNickname()
    {
        ProjectState proj = ProjectManager.GetProject() ?? throw new NullReferenceException();
        return proj.Config.LocalConfig.Data.ProjectNickname;
    }
    private void SetNickname(string nick)
    {
        ProjectState proj = ProjectManager.GetProject() ?? throw new NullReferenceException();
        ProjectLocalConfig config = proj.Config.LocalConfig;

        config.Data.ProjectNickname = nick;
        config.WriteFile();
    }

    #endregion
}
