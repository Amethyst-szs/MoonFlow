using Godot;

namespace MoonFlow.Scene.EditorEvent;

public class GraphNodeUndoRedoServer
{
    private readonly GraphCanvas Parent = null;
    private readonly Timer ActivityTimer = null;

    public GraphNodeUndoRedoServer(GraphCanvas context)
    {
        // Setup connection to parent
        Parent = context;
        context.Connect(GraphCanvas.SignalName.ContentModified, Callable.From(OnGraphModified));

        // Setup timer
        ActivityTimer = new()
        {
            OneShot = true,
            WaitTime = 1.5F,
            Autostart = false,
        };

        context.AddChild(ActivityTimer);
        ActivityTimer.Timeout += OnTimerTimeout;
    }

    #region Implementation

    public void RegisterEntry()
    {
        // GD.Print("Entry register placeholder");
    }

    public void Undo()
    {
        // GD.Print("Undo placeholder");
    }

    public void Redo()
    {
        // GD.Print("Redo placeholder");
    }

    #endregion

    #region Signals

    public void OnGraphModified() { ActivityTimer.Start(); }
    public void OnTimerTimeout() { RegisterEntry(); }

    #endregion
}