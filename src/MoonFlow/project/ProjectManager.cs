using System;
using System.IO;
using System.Threading.Tasks;
using Godot;

using MoonFlow.Scene;
using MoonFlow.Scene.Main;

namespace MoonFlow.Project;

public static class ProjectManager
{
    private static ProjectState Project = null;
    private const string ProjectFileName = "Project.MoonFlow";

    public static MainSceneRoot SceneRoot { get; set; } = null;

    // ====================================================== //
    // ============== Open Project by Directory ============= //
    // ====================================================== //

    public enum ProjectManagerResult
    {
        OK,
        INVALID_PATH,
        NO_PROJECT_FILE,
        INVALID_PROJECT_FILE,
        ROMFS_MISSING_PATH_FOR_PROJECT_VERSION,

        PROJECT_FILE_ALREADY_EXISTS,
    }

    public static ProjectManagerResult TryOpenProject(string path)
    {
        if (!IsValidOpenOrCreate(path))
            return ProjectManagerResult.INVALID_PATH;

        if (!IsProjectConfigExist(ref path, out string projectFilePath))
            return ProjectManagerResult.NO_PROJECT_FILE;

        // Read in config file
        var config = new ProjectConfig(projectFilePath);
        if (!config.IsValid())
            return ProjectManagerResult.INVALID_PROJECT_FILE;

        // Swap active RomfsAccessor to match this project (and error if no valid accessor exists)
        if (!Enum.IsDefined(config.Version))
            return ProjectManagerResult.INVALID_PROJECT_FILE;

        if (!RomfsAccessor.TrySetGameVersion(config.Version))
            return ProjectManagerResult.ROMFS_MISSING_PATH_FOR_PROJECT_VERSION;

        // Close the FrontDoor application if open and open the project loading screen
        var frontDoor = SceneRoot.GetApp<FrontDoor>();
        frontDoor?.AppClose(true);

        var loadScreen = SceneCreator<ProjectLoading>.Create();
        SceneRoot.NodeApps.AddChild(loadScreen);

        // Initilize project
        Project = new(path, config, loadScreen);

        Task task = Task.Run(new Action(Project.InitProject));
        loadScreen.LoadingStart(task);

        return ProjectManagerResult.OK;
    }

    // ====================================================== //
    // ===================== New Project ==================== //
    // ====================================================== //

    public static ProjectManagerResult TryCreateProject(ProjectInitInfo initInfo)
    {
        // Ensure the provided directory is a valid place to create our project
        var path = initInfo.Path;
        if (!IsValidOpenOrCreate(path))
            return ProjectManagerResult.INVALID_PATH;

        // Make sure we aren't making a new project in a folder that already has a project
        if (IsProjectConfigExist(ref path, out string projectFilePath))
            return ProjectManagerResult.PROJECT_FILE_ALREADY_EXISTS;

        // Create project config from init info
        var config = new ProjectConfig(projectFilePath, initInfo);
        if (!config.IsValid())
            return ProjectManagerResult.INVALID_PROJECT_FILE;

        // Open newly created project file
        return TryOpenProject(path);
    }

    // ====================================================== //
    // ================== Common Utilities ================== //
    // ====================================================== //

    private static bool IsValidOpenOrCreate(string path)
    {
        // If there isn't a reference to MoonFlow's MainSceneRoot node at the top of the godot scene, throw exception
        if (SceneRoot == null)
            throw new NullReferenceException("ProjectManager cannot open project without MainSceneRoot loaded!");

        // Ensure this path isn't a romfs directory in the RomfsAccessor
        if (RomfsAccessor.VersionDirectories.ContainsValue(path))
            return false;

        return true;
    }

    private static bool IsProjectConfigExist(ref string path, out string projectFilePath)
    {
        // If the selected directory contains a romfs folder, navigate into this folder
        if (Directory.Exists(path + "romfs/"))
            path += "romfs/";

        // Navigate to project file
        projectFilePath = path;

        // Ensure directory for LocalizedData/Common
        projectFilePath += "LocalizedData/Common/";
        Directory.CreateDirectory(projectFilePath);
        projectFilePath += ProjectFileName;

        // Check if this path contains a project config file
        if (!File.Exists(projectFilePath))
            return false;

        return true;
    }
}