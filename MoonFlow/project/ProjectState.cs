using System;
using System.Threading.Tasks;
using Godot;

using MoonFlow.Scene;
using MoonFlow.Scene.Main;

using MoonFlow.Project.Database;
using MoonFlow.Project.Cache;
using MoonFlow.Addons;
using MoonFlow.Project.FTP;
using System.Linq;
using Nindot;

namespace MoonFlow.Project;

public class ProjectState(string path, ProjectConfig config)
{
    // Initilzation properties
    public string Path { get; private set; } = path;
    public ProjectConfig Config { get; private set; } = config;
    public Task StartupTask = null;

    // Status
    private bool IsInitComplete = false;
    private bool IsWaitingForAcceptance = false;
    public bool IsTemporary { get; private set; } = false;

    // Project Components
    public ProjectMsbpHolder MsgStudioProject { get; private set; } = null;
    public ProjectMessageStudioText MsgStudioText { get; private set; } = null;
    public ProjectLabelCache MsgLabelCache { get; private set; } = null;

    public ProjectEventDataArchiveHolder EventArcHolder { get; private set; } = null;

    public ProjectDatabaseHolder Database { get; private set; } = null;

    public async void InitProject()
    {
        // Check if the project path is the temporary exploration path
        string tempPath = ProjectSettings.GlobalizePath(ProjectManager.TEMPORARY_PROJECT_PATH);
        IsTemporary = Path == tempPath;

        // Close all applications if open and open the project loading screen
        AppSceneServer.ForceCloseAllAppsDeferred();

        Config.GetEngineTarget(out string name, out string hash, out long time);

        var loadScreen = AppSceneServer.CreateAppDeferred<ProjectLoading>();
        loadScreen.LoadingStart(StartupTask, name, hash, time);

        // Publish project's path to the FTP client here to ensure future file transfers can be sourced correctly
        ProjectFtpClient.UpdateLocalProjectDirectory(Path);

        string transLang = EngineSettings.GetSetting<string>("moonflow/localization/translation_language", "USen");
        bool isTransferAll = ProjectFtpClient.CredentialStore.IsTransferAllLanguages;
        ProjectFtpClient.UpdateLanguageConfiguration(Config.GetDefaultLanguage(), transLang, isTransferAll);

        // Wait 200 milliseconds to allow loading screen to appear
        // This isn't nessecary for the code to function, but allows the end-user time to process the scene
        // transation and improves the user experience a bit!
        await Task.Delay(200);

        // Log application and project version
        GD.Print("\n - Starting project initilization...");
        GD.PrintRich("[i]   Local: " + GitInfo.GitVersionName());
        GD.PrintRich("[i]   Project: " + name + '\n');

        // Update MoonFlow.Project globals
        Global.SetDebugMetadataFileOutput(Config.IsDebug());

        // Check if the application is an incompatible version for the project
        IsWaitingForAcceptance = false;
        
        if (!Config.IsEngineTargetOk(GitInfo.GitCommitHash()))
        {
            var appBuildTime = GitInfo.GitCommitUnixTime();

            // If the project's app compile time is later than our own, display outdated message
            if (appBuildTime < time)
            {
                GD.Print("Project load aborted due to outdated application!");
                IsWaitingForAcceptance = true;
                loadScreen.LoadingStopDueToOutdatedApplication();
                return;
            }

            // If we are ahead of the project's app compile time, display upgrade message
            if (appBuildTime > time && !Config.IsAlwaysUpgrade())
            {
                GD.Print("Project uses an older version of MoonFlow, awaiting upgrade acceptance...");

                IsWaitingForAcceptance = true;
                loadScreen.LoadingPauseForUpgradeRequest();
                return;
            }
        }

        InitProjectHandler(loadScreen);
    }

    public void InitProjectAfterDecide(ProjectLoading loadScreen)
    {
        if (!IsWaitingForAcceptance)
            throw new Exception("Method can only be called during upgrade acceptance period");

        IsWaitingForAcceptance = false;

        if (!loadScreen.IsAcceptUpgrade)
        {
            GD.Print("Upgrade rejected, aborting project init");
            ProjectManager.CloseProject();
            return;
        }

        GD.Print("Upgrade accepted");

        if (loadScreen.IsAcceptUpgradeAlways)
        {
            GD.Print("Project will now automatically upgrade version");
            Config.SetAlwaysAcceptUpgradeFlag();
        }

        StartupTask.ContinueWith((_) => InitProjectHandler(loadScreen));
    }

    private void InitProjectHandler(ProjectLoading loadScreen)
    {
        try
        {
            InitProjectContent(loadScreen);
        }
        catch (Exception e)
        {
            loadScreen.LoadingException(e);
        }
    }

    private void InitProjectContent(ProjectLoading loadScreen)
    {
        // Setup MSBP holder
        loadScreen.LoadingUpdateProgress("LOAD_MSBP");
        MsgStudioProject = new(Path, Config);

        // Preload archives for default language
        loadScreen.LoadingUpdateProgress("LOAD_MSBT");
        MsgStudioText = new(Path, Config.GetDefaultLanguage(), loadScreen);

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

        // Complete Initilization
        var gitHash = GitInfo.GitCommitHash();

        if (!Config.IsEngineTargetOk(gitHash))
            InitProjectUpgradingFromOldVersion(loadScreen, gitHash);

        if (Config.IsFirstBoot())
            InitProjectFirstOpen(loadScreen);

        loadScreen.LoadingComplete();
        StartupTask = null;

        ProjectManager.GetObjectDataListing();

        GD.Print("Project initilization successful");
        IsInitComplete = true;
    }

    private void InitProjectUpgradingFromOldVersion(ProjectLoading loadScreen, string gitHash)
    {
        loadScreen.LoadingUpdateProgress("LOAD_PROJECT_UPGRADE");

        // Finalize project upgrading
        Config.EnsureSignature();
        Config.SetEngineTarget(GitInfo.GitVersionName(), gitHash, GitInfo.GitCommitUnixTime());
        Config.WriteFile();
    }
    private void InitProjectFirstOpen(ProjectLoading loadScreen)
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

        // Finalize first open changes
        Config.EnsureSignature();
        Config.ClearFirstBootFlag();
        Config.WriteFile();
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