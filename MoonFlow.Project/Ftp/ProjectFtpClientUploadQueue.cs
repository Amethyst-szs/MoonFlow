using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Godot;

using Nindot;

using FluentFTP;

namespace MoonFlow.Project.FTP;

public static partial class ProjectFtpClient
{
    private readonly static Queue<IProjectFtpQueueItem> RemoteQueue = [];
    private static IProjectFtpQueueItem RemoteCurrent = null;

    private static bool IsQueueActive = false;

    private static void TryStartupQueue()
    {
        if (IsQueueActive || RemoteQueue.Count == 0) return;

        IsQueueActive = true;
        StatusIndicator?.SetStatusActive();

        Task.Run(TaskRunnerQueueProcessing).ContinueWith(TaskRunnerQueueEnded);
    }

    private static async Task TaskRunnerQueueProcessing()
    {
        if (ProjectPath == null || ProjectPath == string.Empty)
            throw new NullReferenceException("No project path defined!");

        while (RemoteQueue.Count > 0)
        {
            RemoteCurrent = RemoteQueue.Dequeue();
            await RemoteCurrent.Process();

            RemoteCurrent = null;
        }
    }
    private static async Task TaskRunnerQueueEnded(Task task)
    {
        // Lil' bit of black magic, creates an empty godot object and uses it to align
        // execution with the process frame
        var processAlignment = new GodotObject();
        await processAlignment.ToSignal(Engine.GetMainLoop(), "process_frame");
        processAlignment.Free();

        // Finalize the completion of the queue
        IsQueueActive = false;
        StatusIndicator.SetStatusConnected();

        // Check to see if the queue needs to be started again in case a push occured during wait
        TryStartupQueue();
    }

    #region Utility

    private static bool IsQueueItemAlreadyExist(IProjectFtpQueueItem item)
    {
        if (RemoteCurrent != null && !item.IsUnique(RemoteCurrent))
            return true;

        if (RemoteQueue.Any((compare) => !item.IsUnique(compare)))
            return true;
        
        return false;
    }

    #endregion
}