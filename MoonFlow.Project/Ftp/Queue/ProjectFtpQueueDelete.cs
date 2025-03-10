using System;
using System.Threading.Tasks;
using Godot;

using FluentFTP;
using System.Linq;

namespace MoonFlow.Project.FTP;

internal struct ProjectFtpQueueDelete(string path, bool isRootPath = false, bool isEmptyDirectory = false) : IProjectFtpQueueItem
{
    internal string Path = path;
    internal bool IsRootPath = isRootPath;
    internal bool IsEmptyDirectory = isEmptyDirectory;

    public readonly async Task<bool> Process()
    {
        string remote = Path;

        if (!IsRootPath)
            remote = ProjectFtpClient.CalcServerPathFromProjectPath(Path);

        if (await ProjectFtpClient.Client.FileExists(remote))
        {
            await ProjectFtpClient.Client.DeleteFile(remote);

            if (DebugFsFtpLogging)
                GD.Print("FTP: Deleted file " + Path.Split(['/', '\\']).Last());

            return true;
        }

        if (await ProjectFtpClient.Client.DirectoryExists(remote))
        {
            if (!IsEmptyDirectory)
                await ProjectFtpClient.Client.DeleteDirectory(remote);
            else
                await ProjectFtpClient.Client.EmptyDirectory(remote, FtpListOption.Auto);

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