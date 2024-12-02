using System.Collections.Generic;
using System.Threading.Tasks;
using MoonFlow.Scene;

namespace MoonFlow.Project;

public class ProjectState(string path, ProjectConfig config, ProjectLoading loadScreen)
{
    // Initilzation properties
    public string Path { get; private set; } = path;
    private ProjectConfig Config = config;
    private ProjectLoading LoadingScreen = loadScreen;

    // Status
    private bool IsInitComplete = false;

    // Project Components
    public ProjectMsbpHolder MsgStudioProject = null;
    private Dictionary<string, ProjectMsbtArchives> MsgArchives = [];

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
        string lang = Config.DefaultLanguage;
        MsgArchives.Add(lang, new(Path, lang));

        // Complete Initilization
        LoadingScreen.LoadingComplete();
        Config = null;
        LoadingScreen = null;
        IsInitComplete = true;
    }

    public bool IsReady() { return IsInitComplete; }
}