using Godot;
using System;
using System.Threading.Tasks;

using Nindot.LMS.Msbt;

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

	[Export, ExportSubgroup("Project Version Info")]
	private GridContainer GridVersionDetails = null;
	[Export]
	private RichTextLabel LabelOutdatedLocal = null;
	[Export]
	private RichTextLabel LabelOutdatedRemote = null;

	[Export, ExportSubgroup("Outdated Application")]
	private VBoxContainer ContainerOutdated = null;

	[Export, ExportSubgroup("Upgrade Required")]
	private VBoxContainer ContainerUpgrade = null;

	public bool IsAcceptUpgrade { get; private set; } = false;
	public bool IsAcceptUpgradeAlways { get; private set; } = false;

	[Export, ExportSubgroup("Init Exception")]
	private VBoxContainer ContainerException = null;
	[Export]
	private Label LabelException = null;
	[Export]
	private MsbtEntryParserExceptionInfoScene MsbtEntryParserInfoScene = null;

	public override void _Ready()
	{
		LoadingUpdateProgress("START");
		SetVisibleContainer(ContainerStatus);
	}

	public void LoadingStart(Task task, string remoteName, string remoteHash, long remoteTimeU)
	{
		LoadingTask = task;

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

		// Push attention to display server
		if (!DisplayServer.WindowIsFocused())
            DisplayServer.WindowRequestAttention();

		// Open home page application
		AppSceneServer.CreateAppDeferred<HomeRoot>();
		CallDeferred(AppScene.MethodName.AppCloseForce);
	}

	private static void OnButtonGoToFrontDoorPressed() { ProjectManager.CloseProject(); }

	#region Outdated

	public void LoadingStopDueToOutdatedApplication()
	{
		// Update containers
		IconStatus.SetDeferred(MoonFlowStatusIcon.PropertyName.AnimationState,
			(int)MoonFlowStatusIcon.AnimationStates.IDLE
		);
		SetVisibleContainer(ContainerOutdated);
	}

	#endregion

	#region Upgrade

	public void LoadingPauseForUpgradeRequest()
	{
		IconStatus.SetDeferred(MoonFlowStatusIcon.PropertyName.AnimationState,
			(int)MoonFlowStatusIcon.AnimationStates.ACTIVE
		);
		SetVisibleContainer(ContainerUpgrade);
	}

	public void OnUpgradeDecisionCancelAndGoFrontDoor()
	{
		IsAcceptUpgrade = false;
		IsAcceptUpgradeAlways = false;

		ProjectManager.GetProject().InitProjectAfterDecideUpgrade(this);
	}
	public void OnUpgradeDecisionAcceptOnce()
	{
		IsAcceptUpgrade = true;
		IsAcceptUpgradeAlways = false;

		IconStatus.SetDeferred(MoonFlowStatusIcon.PropertyName.AnimationState,
			(int)MoonFlowStatusIcon.AnimationStates.SPINNING
		);
		SetVisibleContainer(ContainerStatus);

		ProjectManager.GetProject().InitProjectAfterDecideUpgrade(this);
	}
	public void OnUpgradeDecisionAcceptAlways()
	{
		IsAcceptUpgrade = true;
		IsAcceptUpgradeAlways = true;

		IconStatus.SetDeferred(MoonFlowStatusIcon.PropertyName.AnimationState,
			(int)MoonFlowStatusIcon.AnimationStates.SPINNING
		);
		SetVisibleContainer(ContainerStatus);

		ProjectManager.GetProject().InitProjectAfterDecideUpgrade(this);
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

		IconStatus.SetDeferred(MoonFlowStatusIcon.PropertyName.AnimationState,
			(int)MoonFlowStatusIcon.AnimationStates.IDLE
		);

		SetVisibleContainer(ContainerException);

		if (e != null)
			LabelException.CallDeferred("set", ["text", GetExceptionAsString(e)]);
		
		if (e is MsbtEntryParserException parserException)
			LoadingExceptionFromMsbtParser(parserException);
	}

	private void LoadingExceptionFromMsbtParser(MsbtEntryParserException e)
	{
		MsbtEntryParserInfoScene.AppearInfo(e);
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

		// Update version detail grid visiblity
		GridVersionDetails.CallDeferred(MethodName.SetVisible,
			container == ContainerOutdated || container == ContainerUpgrade
		);
	}

	#endregion
}
