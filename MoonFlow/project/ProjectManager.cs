using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;

using Nindot.Al.SMO;
using Nindot.LMS.Msbp;

using MoonFlow.Scene;
using MoonFlow.Scene.Main;
using MoonFlow.Project.Database;

namespace MoonFlow.Project;

public static class ProjectManager
{
    private static ProjectState Project = null;
    private const string ProjectFileName = ".mfproj";

    public static MainSceneRoot SceneRoot { get; set; } = null;

    // ====================================================== //
    // =================== User Utilities =================== //
    // ====================================================== //

    public static bool IsProjectExist() { return Project != null; }
    public static ProjectState GetProject() { return Project; }
    public static RomfsValidation.RomfsVersion GetProjectVersion()
    {
        if (Project == null) return RomfsValidation.RomfsVersion.INVALID_VERSION;
        return Project.Config.Data.Version;
    }
    public static bool IsProjectDebug()
    {
        if (!OS.IsDebugBuild()) return false;
        if (Project == null) return false;
        return Project.Config.Data.IsDebugProject;
    }

    public static ProjectMsbpHolder GetMSBPHolder() { return Project?.MsgStudioProject; }
    public static SarcMsbpFile GetMSBP() { return Project?.MsgStudioProject?.Project; }

    public static ProjectMessageStudioText GetMSBT() { return Project?.MsgStudioText; }
    public static ProjectLanguageHolder GetMSBTArchives() { return Project?.MsgStudioText?.DefaultLanguage; }
    public static ProjectLanguageHolder GetMSBTArchives(string lang) { return Project?.MsgStudioText[lang]; }
    public static ProjectLanguageMetaHolder GetMSBTMetaHolder()
    {
        return GetMSBTMetaHolder(Project?.Config?.Data?.DefaultLanguage);
    }
    public static ProjectLanguageMetaHolder GetMSBTMetaHolder(string lang)
    {
        if (Project == null || Project.MsgStudioText == null || lang == null)
            return null;
        
        Project.MsgStudioText.TryGetValue(lang, out ProjectLanguageHolder langHolder);
        if (langHolder == null)
            throw new Exception("Invalid Language: " + lang);

        return langHolder.Metadata;
    }

    public static ProjectDatabaseHolder GetDB() { return Project?.Database; }

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
        GD.Print("Opening project at ", path);

        // Setup out variable
        version = RomfsValidation.RomfsVersion.INVALID_VERSION;

        if (!IsValidOpenOrCreate(ref path))
            return ProjectManagerResult.INVALID_PATH;

        if (!IsProjectConfigExist(ref path, out string projectFilePath))
            return ProjectManagerResult.NO_PROJECT_FILE;

        // Read in config file
        var config = new ProjectConfig(projectFilePath);

        // Swap active RomfsAccessor to match this project (and error if no valid accessor exists)
        if (!Enum.IsDefined(config.Data.Version))
            return ProjectManagerResult.INVALID_PROJECT_FILE;

        version = config.Data.Version;
        if (!RomfsAccessor.TrySetGameVersion(version))
            return ProjectManagerResult.ROMFS_MISSING_PATH_FOR_PROJECT_VERSION;

        // Initilize project
        Project = new(path, config);

        Task task = new(() => Project.InitProject(SceneRoot));
        Project.StartupTask = task;
        task.Start();

        return ProjectManagerResult.OK;
    }

    public static void CloseProject()
    {
        if (Project == null || SceneRoot == null)
            return;

        Project = null;

        SceneRoot.ForceCloseAllApps();
        var frontDoor = SceneCreator<FrontDoor>.Create();
        SceneRoot.NodeApps.AddChild(frontDoor);
    }

    // ====================================================== //
    // ===================== New Project ==================== //
    // ====================================================== //

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
        return ProjectManagerResult.OK;
    }

    // ====================================================== //
    // ================== Common Utilities ================== //
    // ====================================================== //

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
}