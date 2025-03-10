using Godot;
using MoonFlow.Project;
using MoonFlow.Scene.Settings;

namespace MoonFlow.Scene.Main;

public partial class ActionbarProject : ActionbarItemBase
{
	private enum MenuIds : int
	{
		PROJECT_OPEN_IN_EXPLORER = 0,
		PROJECT_RELOAD = 1,
		PROJECT_CLOSE = 2,

		OPEN_ENGINE_SETTINGS = 3,
	}

	public override void _Ready()
	{
		base._Ready();

		AssignFunction((int)MenuIds.PROJECT_OPEN_IN_EXPLORER, OnProjectOpenInExpolorerPressed);
		AssignFunction((int)MenuIds.PROJECT_RELOAD, OnProjectReloadPressed, "home_actionbar_reload");
		AssignFunction((int)MenuIds.PROJECT_CLOSE, OnProjectClosePressed, "home_actionbar_close");
		AssignFunction((int)MenuIds.OPEN_ENGINE_SETTINGS, OnEngineSettingsPressed);
	}

	private async void OnProjectReloadPressed()
	{
		var isValidReload = await AppSceneServer.TryCloseAllApps();
		if (!isValidReload)
			return;

		var path = ProjectManager.GetProject().Path;
		ProjectManager.TryOpenProject(path, out _);
	}

	private async void OnProjectClosePressed()
	{
		var isValidClose = await AppSceneServer.TryCloseAllApps();
		if (!isValidClose)
			return;

		ProjectManager.CloseProject();
	}

	private void OnProjectOpenInExpolorerPressed()
	{
		if (ProjectManager.IsProjectExist())
			OS.ShellShowInFileManager(ProjectManager.GetPath());

		// switch(DisplayServer.GetName())
		// {
		// 	case "Windows":

		// 		break;
		// 	default:
		// 		DisplayServer.FileDialogShow("Project", ProjectManager.GetPath(), null, false, DisplayServer.FileDialogMode.OpenAny, [], Callable.From(null));
		// 		break;
		// }
	}

	private static void OnEngineSettingsPressed() { AppSceneServer.CreateApp<EngineSettingsApp>(); }
}
