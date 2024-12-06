using System;
using System.Threading.Tasks;
using Godot;

namespace MoonFlow.Async;

public class AsyncRunner
{
    private AsyncDisplay Display = null;
    public Task Task { get; private set; } = null;

    public AsyncRunner(Action task)
    {
        Task.Run(task);
        return;
    }
    public AsyncRunner(Action<AsyncDisplay> action, AsyncDisplay.Type type)
    {
        Display = AsyncDisplay.Instantiate(type);
        if (Display == null)
            throw new NullReferenceException("ProjectManager doesn't have scene reference!");

        Display.TreeExiting += OnDisplayNodeFree;

        Task = new Task(() => action.Invoke(Display));
        Task.Start();
        Task.ContinueWith(Finished);
    }

    public static void Run(Action task)
    {
        _ = new AsyncRunner(task);
    }
    public static AsyncRunner Run(Action<AsyncDisplay> task, AsyncDisplay.Type type)
    {
        var run = new AsyncRunner(task, type);
        return run;
    }

    private void Finished(Task task)
    {
        if (Display == null)
        {
            if (task.Exception != null)
                throw task.Exception.GetBaseException();

            return;
        }

        if (task.Exception != null)
        {
            Display.Exception = task.Exception.GetBaseException();
            Display.CallDeferred("OnTaskException");
            return;
        }

        Display.CallDeferred("OnTaskSuccessful");
    }

    private void OnDisplayNodeFree()
    {
        Display = null;
    }
}