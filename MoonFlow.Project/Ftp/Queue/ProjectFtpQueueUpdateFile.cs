using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Godot;

using FluentFTP;

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

        // Ensure the file isn't being reserved by any other process
        if (!await TryWaitForFileAccess(Path))
        {
            GD.Print("FTP: Skipping " + Path.Split(['/', '\\']).Last(), " due to being inaccessible by this process");
            return false;
        }

        // Create callback holder and await upload completion
        var prog = ProjectFtpClient.TryCreateProgressCallbackHolder(Callback);
        await ProjectFtpClient.Client.UploadFile(Path, remote, FtpRemoteExists.Overwrite, true, FtpVerify.Retry, prog);

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

    #region Utilities

    private static async Task<bool> TryWaitForFileAccess(string path, int maxAttempts = 8)
    {
        var info = new FileInfo(path);
        var attempts = 0;

        while (attempts < maxAttempts)
        {
            try
            {
                using FileStream stream = info.Open(FileMode.Open, System.IO.FileAccess.Read, FileShare.None);
                stream.Close();
            }
            catch (IOException)
            {
                attempts++;
                await Task.Delay(200);
            }

            break;
        }

        return attempts < maxAttempts;
    }

    #endregion
}