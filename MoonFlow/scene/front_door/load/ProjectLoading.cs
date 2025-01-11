using Godot;
using System;
using System.Threading.Tasks;

using MoonFlow.Scene.Home;
using MoonFlow.Scene.Main;
using MoonFlow.Project;

namespace MoonFlow.Scene;

[Icon("res://asset/app/icon/front_door_loading.png")]
[ScenePath("res://scene/front_door/load/project_loading.tscn")]
public partial class ProjectLoading : AppScene, IProjectLoadingScene
{
	private Task LoadingTask = null;
	private Exception TaskException = null;

	[Export, ExportGroup("Internal References")]
	private Label LabelProgress = null;
	[Export]
	private VBoxContainer ContainerStatus = null;
	[Export]
	private MoonFlowStatusIcon IconStatus = null;

	[Export]
	private VBoxContainer ContainerException = null;
	[Export]
	private Label LabelException = null;

	public override void _Ready()
	{
		LoadingUpdateProgress("START");
		ContainerException.Hide();
	}

	public override void _Process(double _)
	{
		if (!ContainerException.Visible && LoadingTask.Exception != null)
			LoadingException(LoadingTask.Exception);
	}

	public void LoadingStart(Task task)
	{
		LoadingTask = task;
	}

	// Translation keys found in loading_messages.gd
	public void LoadingUpdateProgress(string key)
	{
		string m = TranslationServer.Translate(key, "PROJECT_LOADING");
		LabelProgress.CallDeferred("set", ["text", m]);
	}
	public void LoadingUpdateProgress(string key, string suffix)
	{
		string m = TranslationServer.Translate(key, "PROJECT_LOADING");
		LabelProgress.CallDeferred("set", ["text", m + " ( " + suffix + " )"]);
	}

	public void LoadingComplete()
	{
		LoadingUpdateProgress("END");

		// Open home page application
		var app = SceneCreator<HomeRoot>.Create();
		Scene.NodeApps.CallDeferred("add_child", app);

		// Close the loading screen application
		AppClose(true);
	}

	// ====================================================== //
	// ================== Exception Events ================== //
	// ====================================================== //

	public void LoadingException(Exception e)
	{
		TaskException = e;

		IconStatus.AnimationState = MoonFlowStatusIcon.AnimationStates.IDLE;
		ContainerStatus.CallDeferred("hide");
		ContainerException.CallDeferred("show");

		if (e != null)
			LabelException.CallDeferred("set", ["text", GetExceptionAsString(e)]);
	}

	private void OnButtonExceptionQuitPressed()
	{
		AppClose(true);

		var frontDoor = SceneCreator<FrontDoor>.Create();
		Scene.NodeApps.AddChild(frontDoor);
	}

	private void OnButtonExceptionCopyLogPressed()
	{
		DisplayServer.ClipboardSet(GetExceptionAsString(TaskException));
		GD.Print("Copied " + TaskException.GetType().Name + " to clipboard");
	}

	private static string GetExceptionAsString(Exception e)
	{
		return e.Message + '\n' + e.Source + '\n' + e.TargetSite + "\n\n" + e.StackTrace;
	}
}
