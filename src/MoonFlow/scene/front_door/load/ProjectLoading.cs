using Godot;
using Godot.Collections;
using System;

using MoonFlow.Project;
using System.Threading.Tasks;

namespace MoonFlow.Scene;

[Icon("res://asset/nindot/lms/icon/PictureFont_7B.png")]
[ScenePath("res://scene/front_door/load/project_loading.tscn")]
public partial class ProjectLoading : AppScene
{
	[Export] public string Msg_Starting { get; private set; } = "";
	[Export] public string Msg_Temp { get; private set; } = "";

	private Task LoadingTask = null;

	private Label LabelProgress = null;
	private ScrollContainer ContainerException = null;
	private Label LabelException = null;

    public override void _Ready()
    {
        LabelProgress = GetNode<Label>("%Label_ProgressTask");
		ContainerException = GetNode<ScrollContainer>("%Scroll_LoadException");
		LabelException = GetNode<Label>("%Label_Exception");

		LabelProgress.Text = Msg_Starting;
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

    public void LoadingUpdateProgress(string msg)
	{
		LabelProgress.CallDeferred("set", ["text", msg]);
	}

	public void LoadingComplete()
	{
		GD.Print("Loading Complete");
	}

	// ====================================================== //
	// ================== Exception Events ================== //
	// ====================================================== //

	public void LoadingException(AggregateException e)
	{
		var eb = e.GetBaseException();

		ContainerException.CallDeferred("show");
		LabelException.CallDeferred("set", ["text", eb.Message + '\n' + eb.Source + '\n' + eb.StackTrace]);
	}

	private void OnButtonExceptionQuitPressed()
	{
        AppClose(true);

        var frontDoor = SceneCreator<FrontDoor>.Create();
        Scene.NodeApps.AddChild(frontDoor);
	}
}
