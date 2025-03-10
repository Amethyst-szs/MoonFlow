using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Godot;

using FluentFTP;

namespace MoonFlow.Project.FTP;

internal struct ProjectFtpQueueUploadAll(string projPath, EventHandler<FtpProgress> callback = null) : IProjectFtpQueueItem
{
    internal EventHandler<FtpProgress> Callback = callback;

    public readonly async Task<bool> Process()
    {
        // Calculate remote path from local path
        string local = projPath;
        string remote = ProjectFtpClient.CredentialStore.WorkingDirectory;

        // Create callback holder and await upload completion
        var prog = ProjectFtpClient.TryCreateProgressCallbackHolder(Callback);

        if (DebugFsFtpLogging)
            GD.Print("FTP: Beginning entire project upload to " + remote);

        await ProjectFtpClient.Client.UploadDirectory(local, remote, FtpFolderSyncMode.Mirror, FtpRemoteExists.Overwrite, FtpVerify.Retry, null, prog);

        if (DebugFsFtpLogging)
            GD.Print("FTP: Uploaded to " + remote + " completed!");

        return true;
    }

    public readonly string GetPath() => null;
    public readonly void SetPath(string path) {}

    public readonly EventHandler<FtpProgress> GetCallback() => Callback;
    public readonly bool IsUnique(IProjectFtpQueueItem comparison)
    {
        return GetType() != comparison.GetType();
    }
}