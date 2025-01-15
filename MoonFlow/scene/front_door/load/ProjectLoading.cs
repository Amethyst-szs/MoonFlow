using Godot;
using System;
using System.Threading.Tasks;

using MoonFlow.Scene.Home;
using MoonFlow.Scene.Main;
using MoonFlow.Project;
using MoonFlow.Addons;

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
	private MoonFlowStatusIcon IconStatus = null;
	[Export]
	private VBoxContainer ContainerStatus = null;
	[Export]
	private Godot.Collections.Array<VBoxContainer> Containers = [];

	[Export, ExportSubgroup("Outdated Application")]
	private VBoxContainer ContainerOutdated = null;
	[Export]
	private RichTextLabel LabelOutdatedLocal = null;
	[Export]
	private RichTextLabel LabelOutdatedRemote = null;

	[Export, ExportSubgroup("Upgrade Required")]
	private VBoxContainer ContainerUpgrade = null;

	[Export, ExportSubgroup("Init Exception")]
	private VBoxContainer ContainerException = null;
	[Export]
	private Label LabelException = null;

	public override void _Ready()
	{
		LoadingUpdateProgress("START");
		SetVisibleContainer(ContainerStatus);
	}

	public void LoadingStart(Task task)
	{
		LoadingTask = task;
	}

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

	private void OnButtonGoToFrontDoorPressed()
	{
		AppClose(true);

		var frontDoor = SceneCreator<FrontDoor>.Create();
		Scene.NodeApps.AddChild(frontDoor);
	}

	#region Upgrade

	public void LoadingStopDueToOutdatedApplication(string remoteName, string remoteHash, long remoteTimeU)
	{
		// Update containers
		IconStatus.AnimationState = MoonFlowStatusIcon.AnimationStates.IDLE;
		SetVisibleContainer(ContainerOutdated);

		// Setup rich text labels
		const string template = "[hint={2}]{0}\n[i]({1})";

		// Local version
		var localName = GitInfo.GitVersionName();
		if (localName.Contains('('))
			localName = localName.Left(localName.Find('('));
		
		var localHash = GitInfo.GitCommitHash();
		var localTime = GitInfo.GitCommitTime();

		LabelOutdatedLocal.CallDeferred(RichTextLabel.MethodName.AppendText,
			string.Format(template, localName, localTime.ToShortDateString(), localHash)
		);

		// Project version
		if (remoteName.Contains('('))
			remoteName = remoteName.Left(remoteName.Find('('));
		
		var remoteTime = remoteTimeU.UnixToDateTime();

		LabelOutdatedRemote.CallDeferred(RichTextLabel.MethodName.AppendText,
			string.Format(template, remoteName, remoteTime.ToShortDateString(), remoteHash)
		);
	}

	public void LoadingPauseForUpgradeRequest()
	{
		IconStatus.AnimationState = MoonFlowStatusIcon.AnimationStates.ACTIVE;
		SetVisibleContainer(ContainerUpgrade);
	}

	#endregion

	#region Exceptions

	public override void _Process(double _)
	{
		if (!ContainerException.Visible && LoadingTask.Exception != null)
			LoadingException(LoadingTask.Exception);
	}

	public void LoadingException(Exception e)
	{
		TaskException = e;

		IconStatus.AnimationState = MoonFlowStatusIcon.AnimationStates.IDLE;
		SetVisibleContainer(ContainerException);

		if (e != null)
			LabelException.CallDeferred("set", ["text", GetExceptionAsString(e)]);
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

	#endregion

	#region Utility

	public void SetVisibleContainer(VBoxContainer container)
	{
		// Hide all other containers
		foreach (var box in Containers)
			if (box != container) box.CallDeferred(MethodName.Hide);

		container.CallDeferred(MethodName.Show);
	}

	#endregion
}
