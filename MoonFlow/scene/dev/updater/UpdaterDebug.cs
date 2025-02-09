using Godot;
using System;

namespace MoonFlow.Scene.Dev;

[ScenePath("res://scene/dev/updater/updater_debug.tscn"), Icon("res://iconS.png")]
public partial class UpdaterDebug : AppScene
{
	private string Url = "";

	private void OnLauncherUpdaterOfficial(string url, int byteSize)
	{
		var app = AppSceneServer.CreateApp<DownloadUpdateApp>(url);
		app.InitUpdate(url, byteSize);

		AppCloseForce();
	}

	private void OnTargetUrlChange(string txt) => Url = txt;
	private void OnLaunchUpdater()
	{
		if (Url == string.Empty)
			return;

		var app = AppSceneServer.CreateApp<DownloadUpdateApp>(Url);
		app.InitUpdate(Url, -1);

		AppCloseForce();
	}
}
