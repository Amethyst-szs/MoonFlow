using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Godot;
using Nindot;

namespace MoonFlow.Project.Database;

public static class GraphicsPresetManager
{
    private static string WorkingPath = null;
    private static SarcFile GraphicsPreset = null;
    private static GraphicsPresetDirectoryWatcher LocalWatcher = null;

    public static void InitManager(string projPath)
    {
        TryDestroyManager();
        
        // Establish working directory
        WorkingPath = CalculateWorkingFolderPath(projPath);
        if (Directory.Exists(WorkingPath))
            Directory.Delete(WorkingPath, true);

        Directory.CreateDirectory(WorkingPath);

        // Grab sarc archive
        GraphicsPreset = FindOrCreateSarcFile(projPath);
        if (GraphicsPreset.Content.Count < 1)
            throw new Exception("Extracted GraphicsPreset file has no contents!");

        // Complete extraction
        ExtractArchive();

        // Setup directory access for watcher and user
        LocalWatcher ??= new();
        LocalWatcher.Attach(WorkingPath);

        OS.ShellShowInFileManager(WorkingPath);
    }
    public static void TryDestroyManager()
    {
        LocalWatcher?.TryDestroy();
        LocalWatcher = null;

        if (WorkingPath != null && WorkingPath.StartsWith(OS.GetUserDataDir()))
            Directory.Delete(WorkingPath, true);

        WorkingPath = null;
        GraphicsPreset = null;
    }

    public static void OnModifiedWorkingDirectory()
    {
        if (WorkingPath != null && GraphicsPreset != null)
            PackArchive();
    }

    #region Extract & Pack

    private static void ExtractArchive()
    {
        foreach (var file in GraphicsPreset.Content)
        {
            string path = string.Format("{0}/{1}", WorkingPath, file.Key);
            File.WriteAllBytesAsync(path, [.. file.Value]);
        }
    }
    private static void PackArchive()
    {
        GraphicsPreset.Content.Clear();

        var fileList = Directory.GetFiles(WorkingPath).Select(s => s.Split(['\\', '/']).Last());
        foreach (var file in fileList)
            GraphicsPreset.Content.Add(file, File.ReadAllBytes(WorkingPath + '/' + file));

        GraphicsPreset.WriteArchive();
    }

    #endregion

    #region Utility

    private static SarcFile FindOrCreateSarcFile(string projPath)
    {
        const string suffix = "/SystemData/GraphicsPreset.szs";
        string projArcPath = projPath + suffix;

        if (File.Exists(projArcPath))
            return SarcFile.FromFilePath(projArcPath);

        if (!RomfsAccessor.TryGetRomfsDirectory(out string romDir))
            throw new Exception("RomfsAccessor could not return directory");

        string romArcPath = romDir + suffix;
        if (File.Exists(romArcPath))
        {
            var file = SarcFile.FromFilePath(romArcPath);
            file.FilePath = projArcPath;
            file.WriteArchive();

            return file;
        }

        throw new FileNotFoundException("Could not find GraphicsPreset file in project or romfs dump");
    }

    private static string CalculateWorkingFolderPath(string projPath)
    {
        string basePath = string.Format("{0}/{1}/", OS.GetUserDataDir(), "tmp").Replace('\\', '/');
        string path = basePath + CalculateWorkingFolderName(projPath);
        return path;
    }
    private static string CalculateWorkingFolderName(string projPath)
    {
        var input = Encoding.UTF8.GetBytes(projPath + "GraphicsPresetManager" + "SALT_zn28MAs");
        byte[] hashValue = MD5.HashData(input);
        return BitConverter.ToString(hashValue).Replace("-", string.Empty);
    }

    #endregion
}