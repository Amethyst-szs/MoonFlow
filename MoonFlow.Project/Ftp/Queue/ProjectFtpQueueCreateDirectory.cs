using System;
using System.Threading.Tasks;
using Godot;

using FluentFTP;
using System.Linq;

namespace MoonFlow.Project.FTP;

internal struct ProjectFtpQueueCreateDirectory(string path) : IProjectFtpQueueItem
{
    internal string Path = path;

    public readonly async Task<bool> Process()
    {
        // Calculate remote path from local path
        string remote = ProjectFtpClient.CalcServerPathFromProjectPath(Path);

        await ProjectFtpClient.Client.CreateDirectory(remote, true);
        
        GD.Print("FTP: Created Directory " + Path.Split(['/', '\\']).Last());
        return true;
    }

    public readonly string GetPath() => Path;
    public void SetPath(string path) => Path = path;
    
    public readonly EventHandler<FtpProgress> GetCallback() => null;
    public readonly bool IsUnique(IProjectFtpQueueItem comparison)
    {
        return GetType() != comparison.GetType() || Path != comparison.GetPath();
    }
}