using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace MoonFlow.Project.Database;

internal class GraphicsPresetDirectoryWatcher
{
    public string Path { get; private set; } = null;
    private FileSystemWatcher Watcher = null;

    public void Attach(string path)
    {
        TryDestroy();

        Path = path;

        // Construct and setup a new watcher
        Watcher = new(path)
        {
            IncludeSubdirectories = false,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.LastWrite
        };

        Watcher.Changed += OnChanged;
        Watcher.Deleted += OnDeleted;
        Watcher.Error += OnError;
    }
    public void TryDestroy()
    {
        // Destroy current file system watcher if it already exists
        Watcher?.Dispose();
        Watcher = null;
    }

    #region Signals

    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
        GraphicsPresetManager.OnModifiedWorkingDirectory();
    }
    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        GraphicsPresetManager.OnModifiedWorkingDirectory();
    }
    private void OnError(object sender, ErrorEventArgs e)
    {
        Console.WriteLine("WARNING: GraphicsPresetDirectoryWatcher reported " + e.GetException().Message);
    }

    #endregion
}