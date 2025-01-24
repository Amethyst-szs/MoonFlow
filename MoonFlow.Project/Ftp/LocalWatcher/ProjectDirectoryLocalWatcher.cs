using System;
using System.IO;
using System.Threading.Tasks;
using Godot;

namespace MoonFlow.Project.FTP;

internal class ProjectDirectoryLocalWatcher
{
    public string Path { get; private set; } = null;

    private FileSystemWatcher Watcher = null;

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
            NotifyFilter =
                  NotifyFilters.FileName
                | NotifyFilters.Attributes
                | NotifyFilters.Size
                | NotifyFilters.CreationTime
                | NotifyFilters.LastWrite
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

        if (OS.IsDebugBuild())
            GD.PrintRich("[i] ⒡ Change ~ " + e.Name);
        
        if ((File.GetAttributes(e.FullPath) & FileAttributes.Directory) == 0)
            ProjectFtpClient.UploadFile(e.FullPath);

    }
    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (OS.IsDebugBuild())
            GD.PrintRich("[i] ⒡ Create + " + e.Name);

        if ((File.GetAttributes(e.FullPath) & FileAttributes.Directory) != 0)
            ProjectFtpClient.PushToQueue<ProjectFtpQueueCreateDirectory>(e.FullPath);
        else
            ProjectFtpClient.UploadFile(e.FullPath);
    }
    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        if (OS.IsDebugBuild())
            GD.PrintRich("[i] ⒡ Delete - " + e.Name);

        ProjectFtpClient.PushToQueue<ProjectFtpQueueDelete>(e.FullPath);
    }
    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        if (OS.IsDebugBuild())
            GD.PrintRich("[i] ⒡ Rename : " + e.OldName + " -> " + e.Name);

        if ((File.GetAttributes(e.FullPath) & FileAttributes.Directory) != 0)
            ProjectFtpClient.RenameDirectory(e.OldFullPath, e.FullPath);
        else
            ProjectFtpClient.RenameFile(e.OldFullPath, e.FullPath);
    }
    private void OnError(object sender, ErrorEventArgs e)
    {
        throw e.GetException();
    }

    #endregion
}