using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace MoonFlow.Project.FTP;

internal class ProjectDirectoryLocalWatcher
{
    public string Path { get; private set; } = null;
    private FileSystemWatcher Watcher = null;

    private static readonly string[] BlacklistFileTypes = [
        ".mfproj",
        ".mfmeta",
        ".mfgraph",

        "_d", // All MoonFlow debug file extensions end with an "_d" suffix
    ];

    public void AttachToProject(string path)
    {
        Path = path;

        // Destroy current file system watcher if it already exists
        if (Watcher != null)
        {
            Watcher.Dispose();
            Watcher = null;
        }

        // Construct and setup a new watcher
        Watcher = new(path)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.LastWrite
        };

        Watcher.Changed += OnChanged;
        Watcher.Created += OnCreated;
        Watcher.Deleted += OnDeleted;
        Watcher.Renamed += OnRenamed;
        Watcher.Error += OnError;
    }

    #region Signals

    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed) return;

        var remote = ProjectFtpClient.CalcServerPathFromProjectPath(e.FullPath);
        if (!IsValidForTransfer(remote))
            return;

        if (DebugFsFtpLogging)
            GD.PrintRich("[i] ⒡ Change ~ " + e.Name);

        if (!File.Exists(e.FullPath))
            return;
        
        if ((File.GetAttributes(e.FullPath) & FileAttributes.Directory) == 0)
            ProjectFtpClient.UploadFile(e.FullPath);

    }
    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (DebugFsFtpLogging)
            GD.PrintRich("[i] ⒡ Create + " + e.Name);

        if ((File.GetAttributes(e.FullPath) & FileAttributes.Directory) != 0)
            ProjectFtpClient.PushToQueue<ProjectFtpQueueCreateDirectory>(e.FullPath);
        else
            ProjectFtpClient.UploadFile(e.FullPath);
    }
    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        if (DebugFsFtpLogging)
            GD.PrintRich("[i] ⒡ Delete - " + e.Name);

        ProjectFtpClient.PushToQueue<ProjectFtpQueueDelete>(e.FullPath);
    }
    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        if (DebugFsFtpLogging)
            GD.PrintRich("[i] ⒡ Rename : " + e.OldName + " -> " + e.Name);

        if ((File.GetAttributes(e.FullPath) & FileAttributes.Directory) != 0)
            ProjectFtpClient.RenameDirectory(e.OldFullPath, e.FullPath);
        else
            ProjectFtpClient.RenameFile(e.OldFullPath, e.FullPath);
    }
    private void OnError(object sender, ErrorEventArgs e)
    {
        Console.WriteLine("WARNING: ProjectDirectoryLocalWatcher reported " + e.GetException().Message);
    }

    #endregion

    #region Utility

    private static bool IsValidForTransfer(string remote)
    {
        if (ProjectFtpClient.CredentialStore.IsTransferAllLanguages)
            return true;
        
        // If the path ends with a blacklisted file format, always skip
        if (BlacklistFileTypes.Any(remote.EndsWith))
            return false;
        
        // If the path isn't part of localized data or is MSBP information, always accept transfer
        if (!remote.Contains("LocalizedData/") || remote.Contains("LocalizedData/Common/"))
            return true;
            
        var main = ProjectFtpClient.CredentialStore.DefaultLanguage;
        var trans = ProjectFtpClient.CredentialStore.TranslationLanguage;

        return remote.Contains("LocalizedData/" + main + "/") || remote.Contains("LocalizedData/" + trans + "/");
    }

    #endregion
}