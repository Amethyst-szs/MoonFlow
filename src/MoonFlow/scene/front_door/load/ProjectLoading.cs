using Godot;
using Godot.Collections;
using System;

using MoonFlow.Project;

namespace MoonFlow.Scene;

[Icon("res://asset/nindot/lms/icon/PictureFont_7B.png")]
[ScenePath("res://scene/front_door/load/project_loading.tscn")]
public partial class ProjectLoading : AppScene
{
	[Export] public string Msg_Starting { get; private set; } = "";
	[Export] public string Msg_Temp { get; private set; } = "";

	public void LoadingUpdateProgress(string msg)
	{
		GetNode<Label>("%Label_ProgressTask").Text = msg;
	}

	public void LoadingComplete()
	{
		GD.Print("Loading Complete");
	}
}
