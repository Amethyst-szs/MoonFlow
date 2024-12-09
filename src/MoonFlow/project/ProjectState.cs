using System.Threading.Tasks;
using Godot;

using MoonFlow.Scene;
using MoonFlow.Scene.Main;

using MoonFlow.Project.Database;

namespace MoonFlow.Project;

public class ProjectState(string path, ProjectConfig config)
{
    // Initilzation properties
    public string Path { get; private set; } = path;
    public ProjectConfig Config { get; private set; } = config;
    public Task StartupTask = null;

    // Status
    private bool IsInitComplete = false;

    // Project Components
    public ProjectMsbpHolder MsgStudioProject { get; private set; } = null;
    public ProjectMessageStudioText MsgStudioText { get; private set; } = null;

    public ProjectDatabaseHolder Database { get; private set; } = null;

    public async void InitProject(MainSceneRoot scene)
    {
        // Close all applications if open and open the project loading screen
        scene.CallDeferred("ForceCloseAllApps");

        var loadScreen = SceneCreator<ProjectLoading>.Create();
        loadScreen.LoadingStart(StartupTask);
        scene.NodeApps.CallDeferred("add_child", loadScreen);

        // Wait 200 milliseconds to allow loading screen to appear
        // This isn't nessecary for the code to function, but allows the end-user time to process the scene
        // transation and improves the user experience a bit!
        await Task.Delay(200);

        // Setup MSBP holder
        loadScreen.LoadingUpdateProgress("LOAD_MSBP");
        MsgStudioProject = new(Path);

        // Preload archives for default language
        loadScreen.LoadingUpdateProgress("LOAD_MSBT");
        MsgStudioText = new(Path, Config.Data.DefaultLanguage);

        // Initilize project database holder
        Database = new(this, loadScreen);

        // Complete Initilization
        loadScreen.LoadingComplete();
        StartupTask = null;
        
        GD.Print("Project initilization successful");
        IsInitComplete = true;
    }

    public bool IsReady() { return IsInitComplete; }

    // ====================================================== //
    // ================== Getter Utilities ================== //
    // ====================================================== //

    public ProjectLanguageHolder GetMsbtArchives()
    {
        return GetMsbtArchives(Config.Data.DefaultLanguage);
    }

    public ProjectLanguageHolder GetMsbtArchives(string lang)
    {
        if (!MsgStudioText.TryGetValue(lang, out ProjectLanguageHolder value))
            return null;

        return value;
    }
}