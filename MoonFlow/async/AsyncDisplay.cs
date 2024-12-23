using System;
using System.Threading.Tasks;
using Godot;

using MoonFlow.Project;

namespace MoonFlow.Async;

[GlobalClass, ScenePath("res://async/async_display.tscn")]
public partial class AsyncDisplay : Control
{
    // ====================================================== //
    // =================== Task Utilities =================== //
    // ====================================================== //

    public void UpdateProgress(int step, int target)
    {
        LabelProgress.SetDeferred(Label.PropertyName.Text, step + " / " + target);
        LabelProgress.CallDeferred(Label.MethodName.Show);

        ProgressBar.SetDeferred(ProgressBar.PropertyName.Value, step);
        ProgressBar.SetDeferred(ProgressBar.PropertyName.MaxValue, target);
    }

    // ====================================================== //
    // ===================== Task Events ==================== //
    // ====================================================== //

    [Signal]
    public delegate void TaskEndedEventHandler();
    [Signal]
    public delegate void TaskSuccessfulEventHandler();
    [Signal]
    public delegate void TaskExceptionEventHandler();

    public Exception Exception = null;

    public async void OnTaskSuccessful()
    {
        LabelProgress.SetDeferred(Label.PropertyName.Text, "Success");

        await Task.Delay(1800);

        EmitSignal(SignalName.TaskSuccessful);
        OnTaskFinished();
    }

    public async void OnTaskException()
    {
        SetDeferred(PropertyName.SelfModulate, new Color(0xFF0000FF));
        GetNode<Label>("%Label_Exception").CallDeferred(Label.MethodName.Show);

        await Task.Delay(4000);

        EmitSignal(SignalName.TaskException);
        OnTaskFinished();
    }

    private async void OnTaskFinished()
    {
        await ToSignal(Engine.GetMainLoop(), "process_frame");

        var tween = CreateTween().SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(this, "modulate", new Color(0), 0.75);
        await ToSignal(tween, Tween.SignalName.Finished);

        EmitSignal(SignalName.TaskEnded);
        QueueFree();
    }

    // ====================================================== //
    // ==================== Initilization =================== //
    // ====================================================== //

    public enum Type : uint
    {
        // Generic types
        FileRead = 0x3A647EFF,
        FileWrite = 0xF46201FF,
        FTP = 0XF1B204FF,

        UpdateProjectLabelCache = 0x3A648EFF,

        SaveMsbtArchives = 0xF46202FF,
        SaveWorldArchives = 0xF46302FF,
        SaveEventFlowGraph = 0xF46303FF,
    }

    private Label LabelTitleKey = null;
    private Label LabelProgress = null;
    private ProgressBar ProgressBar = null;

    public override void _Ready()
    {
        LabelTitleKey = GetNode<Label>("%Label_Title");
        LabelProgress = GetNode<Label>("%Label_Progress");
        ProgressBar = GetNode<ProgressBar>("ProgressBar");
    }

    public static AsyncDisplay Instantiate(Type type)
    {
        if (!IsInstanceValid(ProjectManager.SceneRoot))
            return null;

        var display = SceneCreator<AsyncDisplay>.Create();
        ProjectManager.SceneRoot.NodeAsync.AddChild(display);
        display.Setup(type);
        return display;
    }

    public void Setup(Type type)
    {
        LabelTitleKey.Text = Tr(Enum.GetName(type), "ASYNC_TASK_DISPLAY");
        SelfModulate = new Color((uint)type);
    }
}