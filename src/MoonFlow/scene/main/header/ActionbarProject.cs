using Godot;
using MoonFlow.Project;
using System;

namespace MoonFlow.Scene.Main;

public partial class ActionbarProject : ActionbarItemBase
{
	private enum MenuIds : int
	{
		PROJECT_RELOAD = 0,
		PROJECT_CLOSE = 1,
	}

	public override void _Ready()
	{
		base._Ready();

		AssignFunction((int)MenuIds.PROJECT_RELOAD, OnProjectReloadPressed, "home_actionbar_reload");
		AssignFunction((int)MenuIds.PROJECT_CLOSE, OnProjectClosePressed, "home_actionbar_close");
	}

    private void OnProjectReloadPressed()
	{
		if (!ProjectManager.SceneRoot.TryCloseAllApps())
			return;
		
		var path = ProjectManager.GetProject().Path;
		ProjectManager.TryOpenProject(path, out _);
	}

	private void OnProjectClosePressed()
	{
		if (!ProjectManager.SceneRoot.TryCloseAllApps())
			return;
		
		ProjectManager.CloseProject();
	}
}
