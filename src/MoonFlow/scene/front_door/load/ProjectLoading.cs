using Godot;
using System;

using MoonFlow.Scene.Home;

using System.Threading.Tasks;

namespace MoonFlow.Scene;

[Icon("res://asset/nindot/lms/icon/PictureFont_7B.png")]
[ScenePath("res://scene/front_door/load/project_loading.tscn")]
public partial class ProjectLoading : AppScene
{
	private Task LoadingTask = null;

	private Label LabelProgress = null;
	private ScrollContainer ContainerException = null;
	private Label LabelException = null;

	public override void _Ready()
	{
		LabelProgress = GetNode<Label>("%Label_ProgressTask");
		ContainerException = GetNode<ScrollContainer>("%Scroll_LoadException");
		LabelException = GetNode<Label>("%Label_Exception");

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
		string m = Tr(key, "PROJECT_LOADING");
		LabelProgress.CallDeferred("set", ["text", m]);
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

	public void LoadingException(AggregateException e)
	{
		var eb = e.GetBaseException();

		ContainerException.CallDeferred("show");
		LabelException.CallDeferred("set", ["text", eb.Message + '\n' + eb.Source + "\n\n" + eb.StackTrace]);
	}

	private void OnButtonExceptionQuitPressed()
	{
		AppClose(true);

		var frontDoor = SceneCreator<FrontDoor>.Create();
		Scene.NodeApps.AddChild(frontDoor);
	}
}
