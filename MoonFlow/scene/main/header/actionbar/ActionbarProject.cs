using System;
using System.IO;
using FluentFTP.Helpers;
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

		PROJECT_CONFIG_RENAME = 3,
		PROJECT_MIRROR_CLONE = 4,

		OPEN_ENGINE_SETTINGS = 5,
	}

	public override void _Ready()
	{
		base._Ready();

		AssignFunction((int)MenuIds.PROJECT_OPEN_IN_EXPLORER, OnProjectOpenInExplorerPressed);
		AssignFunction((int)MenuIds.PROJECT_RELOAD, OnProjectReloadPressed, "home_actionbar_reload");
		AssignFunction((int)MenuIds.PROJECT_CLOSE, OnProjectClosePressed, "home_actionbar_close");

		AssignFunction((int)MenuIds.PROJECT_CONFIG_RENAME, OnProjectRenameRequest);
		AssignFunction((int)MenuIds.PROJECT_MIRROR_CLONE, OnProjectMirrorCloneRequest);

		AssignFunction((int)MenuIds.OPEN_ENGINE_SETTINGS, OnEngineSettingsPressed);
	}

	private async void OnProjectReloadPressed()
	{
		var isValidReload = await AppSceneServer.TryCloseAllApps();
		if (!isValidReload)
			return;

		var path = ProjectManager.GetProject().Path;
		ProjectManager.TryOpenProject(ref path, out _);
	}

	private async void OnProjectClosePressed()
	{
		var isValidClose = await AppSceneServer.TryCloseAllApps();
		if (!isValidClose)
			return;

		ProjectManager.CloseProject();
	}

	private void OnProjectRenameRequest()
	{
		DisplayServer.DialogShow("Placeholder", "Not yet implemented!", ["OK"], Callable.From(null));
	}

	private void OnProjectMirrorCloneRequest()
	{
		DisplayServer.FileDialogShow(
			"Select Clone Destination",
			ProjectManager.GetPath(),
			null,
			false,
			DisplayServer.FileDialogMode.OpenDir,
			[],
			Callable.From(new Action<bool, string[], int>(OnProjectMirrorCloneSubmitted))
		);
	}
	private void OnProjectMirrorCloneSubmitted(bool isAccept, string[] paths, int _)
	{
		if (!isAccept || paths.Length != 1)
			return;
		
		string source = ProjectManager.GetPath();
		string target = paths[0].EnsurePostfix("/");
		if (source == target)
			return;
		
		if (!ProjectManager.IsProjectConfigExist(ref target, out string _, false)) {
			GD.Print("There must already be a MoonFlow project at the clone destination!");
			return;
		}
		
		DirectoryExt.CopyFilesRecursively(source, target);
	}

	private void OnProjectOpenInExplorerPressed()
	{
		if (ProjectManager.IsProjectExist())
			OS.ShellShowInFileManager(ProjectManager.GetPath());
	}

	private static void OnEngineSettingsPressed() { AppSceneServer.CreateApp<EngineSettingsApp>(); }
}
