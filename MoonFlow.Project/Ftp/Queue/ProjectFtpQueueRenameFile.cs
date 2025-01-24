using System;
using System.Threading.Tasks;
using Godot;

using FluentFTP;
using System.Linq;

namespace MoonFlow.Project.FTP;

internal struct ProjectFtpQueueRenameFile(string oldPath, string newPath) : IProjectFtpQueueItem
{
    internal string OldPath = oldPath;
    internal string NewPath = newPath;

    public readonly async Task<bool> Process()
    {
        // Calculate remote path from local path
        string remoteOld = ProjectFtpClient.CalcServerPathFromProjectPath(OldPath);
        string remoteNew = ProjectFtpClient.CalcServerPathFromProjectPath(NewPath);

        if (!await ProjectFtpClient.Client.FileExists(remoteOld))
            return false;
        
        if (await ProjectFtpClient.Client.FileExists(remoteNew))
            return false;

        await ProjectFtpClient.Client.Rename(remoteOld, remoteNew);

        GD.Print("FTP: Renamed file " + NewPath.Split(['/', '\\']).Last());
        return true;
    }

    public readonly string GetPath() => NewPath;
    public void SetPath(string path) => throw new NotImplementedException();

    public readonly EventHandler<FtpProgress> GetCallback() => null;
    public readonly bool IsUnique(IProjectFtpQueueItem comparison)
    {
        return GetType() != comparison.GetType() || NewPath != comparison.GetPath();
    }
}