using System;
using System.Threading.Tasks;
using Godot;

using FluentFTP;
using System.Linq;

namespace MoonFlow.Project.FTP;

internal struct ProjectFtpQueueUpdateFile(string path, bool isCompare = false, EventHandler<FtpProgress> callback = null) : IProjectFtpQueueItem
{
    internal string Path = path;
    internal bool IsCompareData = isCompare;
    internal EventHandler<FtpProgress> Callback = callback;

    public readonly async Task<bool> Process()
    {
        // Calculate remote path from local path
        string remote = ProjectFtpClient.CalcServerPathFromProjectPath(Path);

        if (IsCompareData)
        {
            // Perform a comparison to ensure upload is actually required
            // Ideally this would use a checksum comparison but sys-ftpd-light does not support
            // comparing files that way. Date modified will pretty much always return true, so we have to
            // rely on file size changing which is super lame but good enough :(
            const FtpCompareOption compareType = FtpCompareOption.Size;

            var result = await ProjectFtpClient.Client.CompareFile(Path, remote, compareType);
            if (result == FtpCompareResult.Equal)
            {
                GD.Print("FTP: Skipping " + Path.Split(['/', '\\']).Last(), " due to likely being identical");
                return false;
            }
        }

        // Create callback holder and await upload completion
        var prog = ProjectFtpClient.TryCreateProgressCallbackHolder(Callback);
        await ProjectFtpClient.Client.UploadFile(Path, remote, FtpRemoteExists.Overwrite, true, FtpVerify.Retry, prog);

        GD.Print("FTP: Uploaded " + Path.Split(['/', '\\']).Last());
        return true;
    }

    public readonly string GetPath() => Path;
    public readonly EventHandler<FtpProgress> GetCallback() => Callback;
    public readonly bool IsUnique(IProjectFtpQueueItem comparison)
    {
        return GetType() != comparison.GetType() || Path != comparison.GetPath();
    }
}