using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;

using static Nindot.RomfsPathUtility;

using MoonFlow.Scene;
using MoonFlow.Scene.Main;
using MoonFlow.Addons;

namespace MoonFlow.Project;

public static partial class ProjectManager
{
    private static ProjectState Project = null;
    private const string ProjectFileName = ".mfproj";

    public static MainSceneRoot SceneRoot { get; set; } = null;

    public enum ProjectManagerResult
    {
        OK,
        INVALID_PATH,
        NO_PROJECT_FILE,
        INVALID_PROJECT_FILE,
        ROMFS_MISSING_PATH_FOR_PROJECT_VERSION,

        PROJECT_FILE_ALREADY_EXISTS,
    }

    #region Open Project

    public static ProjectManagerResult TryOpenProject(string path, out RomfsVersion version)
    {
        GD.Print("Opening project at ", path);

        // Setup out variable
        version = RomfsVersion.INVALID_VERSION;

        if (!IsValidOpenOrCreate(ref path))
            return ProjectManagerResult.INVALID_PATH;

        if (!IsProjectConfigExist(ref path, out string projectFilePath))
            return ProjectManagerResult.NO_PROJECT_FILE;

        // Read in config file
        var config = new ProjectConfig(projectFilePath);

        // Swap active RomfsAccessor to match this project (and error if no valid accessor exists)
        version = config.GetRomfsVersion();
        if (!RomfsAccessor.TrySetGameVersion(version))
            return ProjectManagerResult.ROMFS_MISSING_PATH_FOR_PROJECT_VERSION;

        // Initilize project
        Project = new(path, config);

        Task task = new(() => Project.InitProject());
        Project.StartupTask = task;
        task.Start();

        return ProjectManagerResult.OK;
    }

    #endregion

    #region New Project

    public static ProjectManagerResult TryCreateProject(ProjectInitInfo initInfo)
    {
        GD.Print("Creating project at ", initInfo.Path);
        GD.Print(" - Version: ", Enum.GetName(initInfo.Version));
        GD.Print(" - Default Lang: ", initInfo.DefaultLanguage);

        // Ensure the provided directory is a valid place to create our project
        var path = initInfo.Path;
        if (!IsValidOpenOrCreate(ref path))
            return ProjectManagerResult.INVALID_PATH;

        // Make sure we aren't making a new project in a folder that already has a project
        if (IsProjectConfigExist(ref path, out string projectFilePath))
            return ProjectManagerResult.PROJECT_FILE_ALREADY_EXISTS;

        // Create project config from init info
        var config = new ProjectConfig(projectFilePath, initInfo);
        config.SetEngineTarget(GitInfo.GitVersionName(), GitInfo.GitCommitHash(), GitInfo.GitCommitUnixTime());
        
        config.WriteFile();

        return ProjectManagerResult.OK;
    }

    #endregion

    #region Backend Utility

    public static void CloseProject()
    {
        if (Project == null || SceneRoot == null)
            return;

        Project = null;

        AppSceneServer.ForceCloseAllApps();
        AppSceneServer.CreateApp<FrontDoor>();
    }

    public static bool IsValidOpenOrCreate(ref string path)
    {
        // If there isn't a reference to MoonFlow's MainSceneRoot node at the top of the godot scene, throw exception
        if (SceneRoot == null)
            throw new NullReferenceException("ProjectManager cannot open project without MainSceneRoot loaded!");

        // Replace all backslashes with forward slashes
        path = path.Replace('\\', '/');

        // Make sure the path ends with a slash
        if (!path.EndsWith('/')) path += '/';

        // If this directory contains a directory called romfs, enter that
        if (!Directory.Exists(path))
            return false;

        if (Directory.GetDirectories(path).Any(s => s.EndsWith("romfs")))
            path += "romfs/";

        // Ensure this path isn't a romfs directory in the RomfsAccessor
        string cmpPath = path;
        if (RomfsAccessor.VersionDirectories.Values.Any(s => s.Equals(cmpPath)))
            return false;

        return true;
    }

    public static bool IsProjectConfigExist(ref string path, out string projectFilePath, bool isCreateDir = true)
    {
        // If the selected directory contains a romfs folder, navigate into this folder
        if (Directory.Exists(path + "romfs/"))
            path += "romfs/";

        // Navigate to project file
        projectFilePath = path;

        // Ensure directory for LocalizedData/Common
        projectFilePath += "LocalizedData/Common/";

        if (isCreateDir)
            Directory.CreateDirectory(projectFilePath);

        projectFilePath += ProjectFileName;

        // Check if this path contains a project config file
        if (!File.Exists(projectFilePath))
            return false;

        return true;
    }

    #endregion
}