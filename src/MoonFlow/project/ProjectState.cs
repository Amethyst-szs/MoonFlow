using System.Collections.Generic;
using System.Threading.Tasks;
using MoonFlow.Scene;

namespace MoonFlow.Project;

public class ProjectState(string path, ProjectConfig config, ProjectLoading loadScreen)
{
    // Initilzation properties
    public string Path { get; private set; } = path;
    public ProjectConfig Config { get; private set; } = config;
    private ProjectLoading LoadingScreen = loadScreen;

    // Status
    private bool IsInitComplete = false;

    // Project Components
    public ProjectMsbpHolder MsgStudioProject { get; private set; } = null;
    public ProjectTextHolder MsgStudioText { get; private set; } = null;

    public async void InitProject()
    {
        // Wait 200 milliseconds to allow loading screen to appear
        // This isn't nessecary for the code to function, but allows the end-user time to process the scene
        // transation and improves the user experience a bit!
        await Task.Delay(200);

        // Setup MSBP holder
        LoadingScreen.LoadingUpdateProgress("LOAD_MSBP");
        MsgStudioProject = new(Path);

        // Preload archives for default language
        LoadingScreen.LoadingUpdateProgress("LOAD_MSBT");
        MsgStudioText = new(Path, Config.DefaultLanguage);

        // Complete Initilization
        LoadingScreen.LoadingComplete();
        LoadingScreen = null;
        IsInitComplete = true;
    }

    public bool IsReady() { return IsInitComplete; }

    // ====================================================== //
    // ================== Getter Utilities ================== //
    // ====================================================== //

    public ProjectMsbtArchives GetMsbtArchives(string lang)
    {
        if (!MsgStudioText.TryGetValue(lang, out ProjectMsbtArchives value))
            return null;

        return value;
    }
}