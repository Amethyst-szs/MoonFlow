using System;
using System.Threading.Tasks;
using Godot;

using MoonFlow.Scene;
using MoonFlow.Scene.Main;

using MoonFlow.Project.Database;
using MoonFlow.Project.Cache;
using MoonFlow.Addons;

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
    public ProjectLabelCache MsgLabelCache { get; private set; } = null;

    public ProjectEventDataArchiveHolder EventArcHolder { get; private set; } = null;

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

        // Update MoonFlow.Project globals
        Global.SetDebugMetadataFileOutput(Config.IsDebug());

        // Attempt to init project contents
        try
        {
            InitProjectContent(loadScreen);
        }
        catch(Exception e)
        {
            loadScreen.LoadingException(e);
        }
    }
    private void InitProjectContent(ProjectLoading loadScreen)
    {
        // Setup MSBP holder
        loadScreen.LoadingUpdateProgress("LOAD_MSBP");
        MsgStudioProject = new(Path);

        // Preload archives for default language
        loadScreen.LoadingUpdateProgress("LOAD_MSBT");
        MsgStudioText = new(Path, Config.GetDefaultLanguage());

        var defaultLanguageArcs = MsgStudioText.DefaultLanguage;

        // Initilize project database holder
        Database = new(Path, defaultLanguageArcs, loadScreen);

        // Refresh MSBP database using newly loaded archives and database
        MsgStudioProject.ReloadProjectSources(defaultLanguageArcs, Database);

        // Create label cache
        loadScreen.LoadingUpdateProgress("LOAD_LABEL_CACHE");
        MsgLabelCache = new(defaultLanguageArcs);
        MsgLabelCache.UpdateCache();

        // Prepare event data archive cache
        EventArcHolder = new(Path, loadScreen);

        // Update the project's target engine version
        Config.SetEngineTarget(GitInfo.GitVersionName(), GitInfo.GitCommitHash(), GitInfo.GitCommitUnixTime());

        // Complete Initilization
        if (Config.IsFirstBoot())
        {
            InitProjectFirstOpen(loadScreen);

            Config.ClearFirstBootFlag();
            Config.WriteFile();
        }

        loadScreen.LoadingComplete();
        StartupTask = null;

        GD.Print("Project initilization successful");
        IsInitComplete = true;
    }

    public void InitProjectFirstOpen(ProjectLoading loadScreen)
    {
        // Build metadata table for MSBT files
        int progress = 0;
        foreach (var lang in MsgStudioText)
        {
            // Skip if metadata is already on disk for language
            if (lang.Value.IsMetadataOnDisk())
            {
                progress++;
                continue;
            }

            // Update loading screen with percentage
            float percent = (float)progress / MsgStudioText.Count * 100F;
            loadScreen?.LoadingUpdateProgress("LOAD_FIRST_BOOT_METADATA_BUILDER", string.Format("{0:0}%", percent));

            // Run table builder
            lang.Value.BuildMetadataTableForInit();
            progress++;
        }
    }

    public bool IsReady() { return IsInitComplete; }

    // ====================================================== //
    // ================== Getter Utilities ================== //
    // ====================================================== //

    public ProjectLanguageHolder GetMsbtArchives()
    {
        return GetMsbtArchives(Config.GetDefaultLanguage());
    }

    public ProjectLanguageHolder GetMsbtArchives(string lang)
    {
        if (!MsgStudioText.TryGetValue(lang, out ProjectLanguageHolder value))
            return null;

        return value;
    }
}