using Godot;

using MoonFlow.Project;
using MoonFlow.Scene.Settings;

namespace MoonFlow.Scene.Main;

public partial class ActionbarHelp : ActionbarItemBase
{
	private enum MenuIds : int
	{
		HELP_CURRENT_APP = 0,

		HELP_HOME_LOCAL = 1,
		HELP_HOME_REMOTE = 2,

		HELP_GITHUB_REPO = 3,
		HELP_CREDITS = 4,
		HELP_SUPPORT = 5,
	}

	private WikiAccessorResource DefaultWikiPage = GD.Load<WikiAccessorResource>(
		"res://scene/common/wiki/paths/default.tres"
	);


	public override void _Ready()
	{
		base._Ready();

		AssignFunction((int)MenuIds.HELP_CURRENT_APP, OnHelpCurrentApp, "ui_help");
		AssignFunction((int)MenuIds.HELP_HOME_LOCAL, OnHelpHomeLocal);
		AssignFunction((int)MenuIds.HELP_HOME_REMOTE, OnHelpHomeRemote);
		AssignFunction((int)MenuIds.HELP_GITHUB_REPO, OnHelpOpenGitHubRepo);
		AssignFunction((int)MenuIds.HELP_CREDITS, OnHelpCreditPage);
		AssignFunction((int)MenuIds.HELP_SUPPORT, OnHelpSupportPage);

		SetItemTooltip(GetItemIndex((int)MenuIds.HELP_SUPPORT), Tr("HelpSupport", "HEADER_TOOLTIP"));
	}

    private async void OnHelpCurrentApp()
	{
		var scene = await GetScene();
		var app = scene.GetActiveApp();

		if (app == null)
			DefaultWikiPage.OpenWiki(GetTree());
		
		app.WikiPage.OpenWiki(GetTree());
	}
    private void OnHelpHomeLocal()
	{
		DefaultWikiPage.OpenWikiLocal(GetTree());
	}
    private void OnHelpHomeRemote()
	{
		DefaultWikiPage.OpenWikiRemote();
	}

	private void OnHelpOpenGitHubRepo()
	{
		var path = EngineSettings.GetSetting<string>("moonflow/wiki/home_repository", "");
		if (path == string.Empty) return;

		OS.ShellOpen(path);
	}
	private async void OnHelpCreditPage()
	{
		var scene = await GetScene();

		var app = SceneCreator<FrontDoorContributorApp>.Create();
		app.SetUniqueIdentifier();
		scene.NodeApps.AddChild(app);
	}
	private void OnHelpSupportPage()
	{
		var path = EngineSettings.GetSetting<string>("moonflow/wiki/support_url", "");
		if (path == string.Empty) return;

		OS.ShellOpen(path);
	}
}
