using Godot;
using System;

using MoonFlow.Project;
using MoonFlow.Scene.Settings;

namespace MoonFlow.Scene.Main;

public partial class ActionbarProject : ActionbarItemBase
{
	private enum MenuIds : int
	{
		PROJECT_RELOAD = 0,
		PROJECT_CLOSE = 1,

		OPEN_ENGINE_SETTINGS = 2,
	}

	public override void _Ready()
	{
		base._Ready();

		AssignFunction((int)MenuIds.PROJECT_RELOAD, OnProjectReloadPressed, "home_actionbar_reload");
		AssignFunction((int)MenuIds.PROJECT_CLOSE, OnProjectClosePressed, "home_actionbar_close");
		AssignFunction((int)MenuIds.OPEN_ENGINE_SETTINGS, OnEngineSettingsPressed);
	}

    private async void OnProjectReloadPressed()
	{
		var isValidReload = await ProjectManager.SceneRoot.TryCloseAllApps();
		if (!isValidReload)
			return;
		
		var path = ProjectManager.GetProject().Path;
		ProjectManager.TryOpenProject(path, out _);
	}

	private async void OnProjectClosePressed()
	{
		var isValidClose = await ProjectManager.SceneRoot.TryCloseAllApps();
		if (!isValidClose)
			return;
		
		ProjectManager.CloseProject();
	}

	private void OnEngineSettingsPressed()
	{
		var app = SceneCreator<EngineSettingsApp>.Create();
		app.SetUniqueIdentifier();
		ProjectManager.SceneRoot.NodeApps.AddChild(app);
	}
}
