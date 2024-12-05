using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;

using Nindot.Al.SMO;
using Nindot.LMS.Msbp;

using MoonFlow.Scene;
using MoonFlow.Scene.Main;

namespace MoonFlow.Project;

public static class ProjectManager
{
    private static ProjectState Project = null;
    private const string ProjectFileName = ".mfproj";

    public static MainSceneRoot SceneRoot { get; set; } = null;

    // ====================================================== //
    // =================== User Utilities =================== //
    // ====================================================== //

    public static ProjectState GetProject() { return Project; }
    public static RomfsValidation.RomfsVersion GetProjectVersion() { return Project.Config.Version; }
    public static SarcMsbpFile GetMSBP() { return Project.MsgStudioProject.Project; }
    public static ProjectMessageStudioText GetMSBT() { return Project.MsgStudioText; }
    public static ProjectLanguageHolder GetMSBTArchives() { return Project.MsgStudioText.DefaultLanguage; }
    public static ProjectLanguageHolder GetMSBTArchives(string lang) { return Project.MsgStudioText[lang]; }
    public static ProjectLanguageMetaHolder GetMSBTMetaHolder()
    {
        return GetMSBTMetaHolder(Project.Config.DefaultLanguage);
    }
    public static ProjectLanguageMetaHolder GetMSBTMetaHolder(string lang)
    {
        Project.MsgStudioText.TryGetValue(lang, out ProjectLanguageHolder langHolder);
        if (langHolder == null)
            throw new Exception("Invalid Language: " + lang);

        return langHolder.Metadata;
    }

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

    public static ProjectManagerResult TryOpenProject(string path, out RomfsValidation.RomfsVersion version)
    {
        // Setup out variable
        version = RomfsValidation.RomfsVersion.INVALID_VERSION;

        if (!IsValidOpenOrCreate(ref path))
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

        version = config.Version;
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
        if (!IsValidOpenOrCreate(ref path))
            return ProjectManagerResult.INVALID_PATH;

        // Make sure we aren't making a new project in a folder that already has a project
        if (IsProjectConfigExist(ref path, out string projectFilePath))
            return ProjectManagerResult.PROJECT_FILE_ALREADY_EXISTS;

        // Create project config from init info
        var config = new ProjectConfig(projectFilePath, initInfo);
        if (!config.IsValid())
            return ProjectManagerResult.INVALID_PROJECT_FILE;

        return ProjectManagerResult.OK;
    }

    // ====================================================== //
    // ================== Common Utilities ================== //
    // ====================================================== //

    private static bool IsValidOpenOrCreate(ref string path)
    {
        // If there isn't a reference to MoonFlow's MainSceneRoot node at the top of the godot scene, throw exception
        if (SceneRoot == null)
            throw new NullReferenceException("ProjectManager cannot open project without MainSceneRoot loaded!");

        // Replace all backslashes with forward slashes
        path = path.Replace('\\', '/');

        // Make sure the path ends with a slash
        if (!path.EndsWith('/')) path += '/';

        // If this directory contains a directory called romfs, enter that
        if (Directory.GetDirectories(path).Any(s => s.EndsWith("romfs")))
            path += "romfs/";

        // Ensure this path isn't a romfs directory in the RomfsAccessor
        string cmpPath = path;
        if (RomfsAccessor.VersionDirectories.Values.Any(s => s.Equals(cmpPath)))
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