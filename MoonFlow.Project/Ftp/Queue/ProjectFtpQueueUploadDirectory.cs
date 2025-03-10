using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Godot;

using FluentFTP;

namespace MoonFlow.Project.FTP;

internal struct ProjectFtpQueueUploadDirectory(string path, EventHandler<FtpProgress> callback = null) : IProjectFtpQueueItem
{
    internal string Path = path;
    internal EventHandler<FtpProgress> Callback = callback;

    public readonly async Task<bool> Process()
    {
        // Calculate remote path from local path
        string remote = ProjectFtpClient.CalcServerPathFromProjectPath(Path);

        // Create callback holder and await upload completion
        var prog = ProjectFtpClient.TryCreateProgressCallbackHolder(Callback);

        await ProjectFtpClient.Client.UploadDirectory(Path, remote, FtpFolderSyncMode.Mirror, FtpRemoteExists.Overwrite, FtpVerify.Retry, null, prog);

        if (DebugFsFtpLogging)
            GD.Print("FTP: Uploaded " + Path.Split(['/', '\\']).Last());

        return true;
    }

    public readonly string GetPath() => Path;
    public void SetPath(string path) => Path = path;

    public readonly EventHandler<FtpProgress> GetCallback() => Callback;
    public readonly bool IsUnique(IProjectFtpQueueItem comparison)
    {
        return GetType() != comparison.GetType() || Path != comparison.GetPath();
    }
}