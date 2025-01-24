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
    private readonly static Queue<ProjectFtpQueueItem> UploadQueue = [];
    private static bool IsQueueActive = false;

    private static void PushQueue(string path, EventHandler<FtpProgress> callback = null)
    {
        if (UploadQueue.Any((i) => i.Path == path))
            return;

        UploadQueue.Enqueue(new ProjectFtpQueueItem(path, callback));
        TryStartupQueue();
    }

    private static void TryStartupQueue()
    {
        if (IsQueueActive || UploadQueue.Count == 0) return;

        IsQueueActive = true;
        StatusIndicator?.SetStatusActive();

        Task.Run(TaskRunnerQueueProcessing).ContinueWith(TaskRunnerQueueEnded);
    }

    private static async Task TaskRunnerQueueProcessing()
    {
        if (ProjectPath == null || ProjectPath == string.Empty)
            throw new NullReferenceException("No project path defined!");

        while (UploadQueue.Count > 0)
        {
            var item = UploadQueue.Dequeue();

            // Calculate remote path from local path
            if (!item.Path.StartsWith(ProjectPath))
                throw new Exception("Cannot upload file that is not contained within project!");

            string remote = CredentialStore.TargetDirectory + item.Path.TrimPrefix(ProjectPath);

            // Create callback holder and await upload completion
            var prog = TryCreateProgressCallbackHolder(item.Callback);
            await Client.UploadFile(item.Path, remote, FtpRemoteExists.Overwrite, true, FtpVerify.Retry, prog);
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
}