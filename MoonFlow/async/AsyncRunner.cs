using System;
using System.Threading.Tasks;

namespace MoonFlow.Async;

public class AsyncRunner
{
    private AsyncDisplay Display = null;
    public Task Task { get; private set; } = null;
    public readonly string Id = null;

    private AsyncRunner(Action task, string id)
    {
        Id = id;
        AsyncStatusKeeper.RegisterTaskId(Id);

        Task = new Task(task);
        Task.Start();
        Task.ContinueWith(Finished);
    }
    private AsyncRunner(Action<AsyncDisplay> action, AsyncDisplay.Type type, string id)
    {
        Id = id;
        AsyncStatusKeeper.RegisterTaskId(Id);

        Display = AsyncDisplay.Instantiate(type);
        if (Display == null)
            throw new NullReferenceException("ProjectManager doesn't have scene reference!");

        Display.TreeExiting += OnDisplayNodeFree;

        Task = new Task(() => action.Invoke(Display));
        Task.Start();
        Task.ContinueWith(Finished);
    }

    public static void Run(Action task, string id)
    {
        if (AsyncStatusKeeper.IsTaskIdRunning(id))
            return;
        
        _ = new AsyncRunner(task, id);
    }
    public static AsyncRunner Run(Action<AsyncDisplay> task, AsyncDisplay.Type type, string id)
    {
        if (AsyncStatusKeeper.IsTaskIdRunning(id))
            return null;
        
        var run = new AsyncRunner(task, type, id);
        return run;
    }

    private void Finished(Task task)
    {
        AsyncStatusKeeper.RemoveTaskId(Id);

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