using System;
using System.Threading.Tasks;
using Godot;

using MoonFlow.Project;

namespace MoonFlow.Async;

[GlobalClass, SceneUid("uid://iqyjx7fu40nl")]
public partial class AsyncDisplay : Control
{
    #region Types

    public enum Type : uint
    {
        // Generic types
        FileRead = 0x3A647EFF,
        FileWrite = 0xF46201FF,
        FTP = 0XF1B204FF,

        // Update project info
        UpdateProjectLabelCache = 0x3A648EFF,
        UpdateProjectMsbp = 0x3A648FFF,

        // Save files
        SaveMsbtArchives = 0xF46202FF,
        SaveMsbp = 0xF46203FF,
        SaveWorldArchives = 0xF46302FF,
        SaveEventFlowGraph = 0xF46303FF,

        // Generate databases
        GenerateCheckpointDb = 0x22BB22FF
    }

    #endregion

    #region Init

    [Export, ExportGroup("Internal References")]
    private Label LabelTitleKey = null;
    [Export]
    private Label LabelDescKey = null;
    [Export]
    private Label LabelProgress = null;
    [Export]
    private Label LabelException = null;
    [Export]
    private ProgressBar ProgressBar = null;

    public static AsyncDisplay Instantiate(Type type)
    {
        if (!IsInstanceValid(ProjectManager.SceneRoot))
            return null;

        var display = SceneCreator<AsyncDisplay>.Create();
        ProjectManager.SceneRoot.NodeAlerts.AddChild(display);
        display.Setup(type);
        return display;
    }

    public void Setup(Type type)
    {
        LabelTitleKey.Text = Tr(Enum.GetName(type), "ASYNC_TASK_DISPLAY");
        LabelDescKey.Text = Tr(Enum.GetName(type), "ASYNC_TASK_DISPLAY_DESCRIPTION");
        SelfModulate = new Color((uint)type);
    }

    #endregion

    #region Task Process

    public void UpdateProgress(int step, int target)
    {
        LabelProgress.SetDeferred(Label.PropertyName.Text, step + " / " + target);
        LabelProgress.CallDeferred(Label.MethodName.Show);

        ProgressBar.SetDeferred(ProgressBar.PropertyName.Value, step);
        ProgressBar.SetDeferred(ProgressBar.PropertyName.MaxValue, target);
    }

    [Signal]
    public delegate void TaskEndedEventHandler();
    [Signal]
    public delegate void TaskSuccessfulEventHandler();
    [Signal]
    public delegate void TaskExceptionEventHandler();

    public Exception Exception = null;

    public async void OnTaskSuccessful()
    {
        ProgressBar.SetDeferred(ProgressBar.PropertyName.Value, 1);
        ProgressBar.SetDeferred(ProgressBar.PropertyName.MaxValue, 1);

        LabelProgress.CallDeferred(Label.MethodName.Hide);
        LabelDescKey.SetDeferred(Label.PropertyName.Text, Tr("Success", "ASYNC_TASK_DISPLAY"));

        await Task.Delay(1800);

        EmitSignal(SignalName.TaskSuccessful);
        OnTaskFinished();
    }

    public async void OnTaskException()
    {
        SetDeferred(PropertyName.SelfModulate, new Color(0xFF0000FF));
        LabelDescKey.CallDeferred(Label.MethodName.Hide);
        LabelProgress.CallDeferred(Label.MethodName.Hide);

        LabelException.CallDeferred(Label.MethodName.Show);

        await Task.Delay(10000);

        EmitSignal(SignalName.TaskException);
        OnTaskFinished();
    }

    private async void OnTaskFinished()
    {
        await Extension.WaitProcessFrame(this);

        var tween = CreateTween().SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(this, "modulate", new Color(0), 0.75);
        await ToSignal(tween, Tween.SignalName.Finished);

        EmitSignal(SignalName.TaskEnded);
        QueueFree();
    }

    private void OnTaskCopyExceptionToClipboard()
    {
        DisplayServer.ClipboardSet(GetExceptionAsString(Exception));
    }
    private static string GetExceptionAsString(Exception e)
	{
		return e.Message + '\n' + e.Source + '\n' + e.TargetSite + "\n\n" + e.StackTrace;
	}

    #endregion
}