using System;
using System.Threading.Tasks;
using Godot;

using FluentFTP;
using System.Linq;

namespace MoonFlow.Project.FTP;

internal struct ProjectFtpQueueDelete(string path) : IProjectFtpQueueItem
{
    internal string Path = path;

    public readonly async Task<bool> Process()
    {
        string remote = ProjectFtpClient.CalcServerPathFromProjectPath(Path);

        if (await ProjectFtpClient.Client.FileExists(remote))
        {
            await ProjectFtpClient.Client.DeleteFile(remote);

            if (DebugFsFtpLogging)
                GD.Print("FTP: Deleted file " + Path.Split(['/', '\\']).Last());

            return true;
        }

        if (await ProjectFtpClient.Client.DirectoryExists(remote))
        {
            await ProjectFtpClient.Client.DeleteDirectory(remote);

            if (DebugFsFtpLogging)
                GD.Print("FTP: Deleted directory " + Path.Split(['/', '\\']).Last());

            return true;
        }

        return false;
    }

    public readonly string GetPath() => Path;
    public void SetPath(string path) => Path = path;

    public readonly EventHandler<FtpProgress> GetCallback() => null;
    public readonly bool IsUnique(IProjectFtpQueueItem comparison)
    {
        return GetType() != comparison.GetType() || Path != comparison.GetPath();
    }
}