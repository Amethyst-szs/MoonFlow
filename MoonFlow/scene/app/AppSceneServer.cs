using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MoonFlow.Addons;
using MoonFlow.Project;
using MoonFlow.Scene.Dev;
using MoonFlow.Scene.Main;

namespace MoonFlow.Scene;

public static partial class AppSceneServer
{
    #region Init

    private static Control AppRoot;
    private readonly static List<AppScene> AppList = [];

    public const string CmdlineArgKeyLaunchmode = "--launchmode";
    public const string CmdlineArgValueLaunchmodeAppless = "appless";
    public const string CmdlineArgValueLaunchmodeUpdateReplaceOld = "update_replace";
    public const string CmdlineArgValueLaunchmodeUpdateCleanup = "update_cleanup";
    public const string CmdlineArgValueLaunchmodeUpdateDebug = "update_debug";
    
    public const string CmdlineArgKeyProject = "--project";

    public static void Init(Control appRoot)
    {
        if (AppRoot != null)
            throw new Exception("AppSceneServer already has reference to app root");

        AppRoot = appRoot;

        // Create initial app using command line arguments
        var args = Cmdline.GetArgs();
        args.TryGetValue(CmdlineArgKeyLaunchmode, out string mode);

        switch (mode)
        {
            case CmdlineArgValueLaunchmodeAppless: // Prevent launching any default application
                GD.Print("Launched in appless mode");
                break;
            case CmdlineArgValueLaunchmodeUpdateReplaceOld: // Used to replace old installation during autoupdate process
                CreateApp<ReplaceOldVersionApp>();
                break;
            case CmdlineArgValueLaunchmodeUpdateCleanup: // Remove temporary update files and continue as normal
                DownloadUpdateApp.CleanupTemporaryUpdateFiles();
                CreateApp<FrontDoor>();
                break;
            case CmdlineArgValueLaunchmodeUpdateDebug: // Provides tools for debugging the auto-update system
                CreateApp<UpdaterDebug>();
                break;
            default: // Standard behavior, showing project selection homescreen
                InitStandardLaunchMode();
                break;
        }
    }

    private static void InitStandardLaunchMode()
    {
        var args = Cmdline.GetArgs();

        // If --project is defined, attempt to open project at requested path
        if (args.TryGetValue(CmdlineArgKeyProject, out string path) && path != string.Empty)
        {
            var result = ProjectManager.TryOpenProject(ref path, out _);

            if (result == ProjectManager.ProjectManagerResult.OK)
                GD.Print("Launching into project at " + path);
            else
                GD.PrintErr("--project cmd arg provided, but couldn't open project at path " + path);
        }

        CreateApp<FrontDoor>();
    }

    public static void Destroy()
    {
        if (AppRoot == null)
            throw new NullReferenceException("Cannot destroy AppSceneServer, no app root");

        ForceCloseAllApps();
        AppRoot = null;
    }

    #endregion

    #region Get Utility

    public static IEnumerable<AppScene> GetApps()
    {
        return AppList;
    }

    public static IEnumerable<T> GetApps<T>() where T : AppScene
    {
        return AppList.Where(n => n is T).Cast<T>();
    }

    public static AppScene GetApp<T>()
    {
        foreach (var app in GetApps())
        {
            if (app.GetType() == typeof(T))
                return app;
        }

        return null;
    }

    public static AppScene GetActiveApp()
    {
        foreach (var app in GetApps())
        {
            if (app.Visible)
                return app;
        }

        return null;
    }

    #endregion

    #region Create Utility

    public static T CreateApp<T>() where T : AppScene
    {
        return CreateApp<T>("");
    }
    public static T CreateApp<T>(string uid) where T : AppScene
    {
        var app = SceneCreator<T>.Create();
        app.SetUniqueIdentifier(uid);
        AppRoot.AddChild(app);

        app.Connect(AppScene.SignalName.AppFocused, Callable.From(() => OnAppFocused(app)));
        app.Connect(AppScene.SignalName.AppTitleChanged, Callable.From(() => OnAppTitleChanged(app)));
        app.Connect(AppScene.SignalName.TreeExited, Callable.From(() => OnAppExited(app)));

        AppList.Add(app);
        return app;
    }

    public static T CreateAppDeferred<T>() where T : AppScene
    {
        return CreateAppDeferred<T>("");
    }
    public static T CreateAppDeferred<T>(string uid) where T : AppScene
    {
        var app = SceneCreator<T>.Create();
        app.SetUniqueIdentifier(uid);
        AppRoot.CallDeferred(AppScene.MethodName.AddChild, app);

        app.CallDeferred(AppScene.MethodName.Connect,
            AppScene.SignalName.AppFocused,
            Callable.From(() => OnAppFocused(app))
        );

        app.CallDeferred(AppScene.MethodName.Connect,
            AppScene.SignalName.AppTitleChanged,
            Callable.From(() => OnAppTitleChanged(app))
        );

        app.CallDeferred(AppScene.MethodName.Connect,
            AppScene.SignalName.TreeExited,
            Callable.From(() => OnAppExited(app))
        );

        AppList.Add(app);
        return app;
    }

    #endregion

    #region Focus Utility

    public static void FocusFirstApp()
    {
        var app = GetApps().First();
        var activeApp = GetActiveApp();

        if (app == activeApp || activeApp.IsAppExclusive())
            return;

        FocusApp(app);
    }

    public static void FocusApp(AppScene app)
    {
        // Set which app is being focused (usually "this" unless special window flags say otherwise)
        var focusingApp = app;

        var activeApp = GetActiveApp();
        if (GodotObject.IsInstanceValid(activeApp) && !activeApp.IsQueuedForDeletion())
        {
            // If the active app is exclusive and this app isn't, don't let the focused app change
            if (activeApp.IsAppExclusive() && !app.IsAppExclusive())
                focusingApp = activeApp;

            // If they are both exclusive, pick item with higher index
            else if (activeApp.IsAppExclusive() && app.IsAppExclusive())
            {
                if (activeApp.GetIndex() > focusingApp.GetIndex())
                    focusingApp = activeApp;
            }
        }

        // Select this app's taskbar button
        var scene = ProjectManager.SceneRoot;
        foreach (var node in scene.NodeTaskbar.GetChildren())
        {
            if (node.GetType() != typeof(TaskbarButton))
                continue;

            var button = (TaskbarButton)node;

            if (button.App != focusingApp)
                button.ButtonPressed = false;
            else
                button.ButtonPressed = true;
        }

        // Show only this app's visibility
        foreach (var node in GetApps())
        {
            var control = (Control)node;

            if (control != focusingApp)
            {
                control.Hide();
                control.ProcessMode = Node.ProcessModeEnum.Disabled;
            }
            else
            {
                control.Show();
                control.ProcessMode = Node.ProcessModeEnum.Inherit;
            }
        }

        focusingApp.EmitSignal(AppScene.SignalName.AppFocused);

        // Update header
        scene.NodeHeader.Visible = focusingApp.IsAppShowHeader();
        scene.NodeHeader.EmitSignal(Header.SignalName.AppFocused);

        UpdateActionbarInjectable(focusingApp);
    }

    private static void UpdateActionbarInjectable(AppScene app)
    {
        // Fetch and reset the current actionbar state
        var scene = ProjectManager.SceneRoot;
        var actionbarInjectable = scene.NodeHeader.ActionbarInjectable;

        foreach (var child in actionbarInjectable.GetChildren())
        {
            if (!child.HasMeta("FromAppScene"))
                continue;

            actionbarInjectable.RemoveChild(child);
            child.QueueFree();
        }

        // Ensure we have a list of actionbar scenes to create
        if (app == null || app.ActionbarScenes.Count == 0)
            return;

        // Create all new inject items
        int createdCount = 0;

        foreach (var itemScene in app.ActionbarScenes)
        {
            var item = itemScene.Instantiate();

            try
            {
                if (item is not ActionbarItemBase actionbarItem)
                    throw new Exception(string.Format("{0} has ActionbarScenes entry that is not ActionbarItemBase", app.AppName));

                actionbarItem.Hide();
                actionbarItem.SetMeta("FromAppScene", true);

                actionbarInjectable.AddChild(actionbarItem, true);
                actionbarInjectable.MoveChild(actionbarItem, 2 + createdCount);

                createdCount++;
            }
            catch
            {
                item.QueueFree();
            }
        }
    }

    #endregion

    #region Close Utility

    public static void CloseActiveApp()
    {
        var app = GetActiveApp();
        if (app == null || !app.IsAppAllowUserToClose())
            return;

        app.AppClose(true);
    }

    public static void ForceCloseAllApps()
    {
        foreach (var app in GetApps())
            app.AppClose(true);
    }
    public static void ForceCloseAllAppsDeferred()
    {
        foreach (var app in GetApps())
            app.CallDeferred(AppScene.MethodName.AppCloseForce);
    }

    public static async Task<bool> TryCloseAllApps()
    {
        var process_frame = AppRoot.ToSignal(Engine.GetMainLoop(), "process_frame");

        // Attempt to close all applications
        foreach (var app in GetApps())
        {
            app.AppFocus();
            await process_frame;

            var closeResult = await app.TryCloseFromTreeQuit();
            if (!closeResult)
            {
                GD.Print("App closing cancelled because " + app.AppName + " refused!");
                return false;
            }
        }

        return true;
    }

    #endregion

    #region Signals

    private static void OnAppFocused(AppScene app)
    {
        OnAppTitleChanged(app);
    }
    private static void OnAppTitleChanged(AppScene app)
    {
        if (app != GetActiveApp())
            return;

        // Calculate prefix
        string prefix = ProjectSettings.GetSetting("application/config/name", "MoonFlow").AsString();;

        if (ProjectManager.IsProjectExist())
        {
            var proj = ProjectManager.GetProject() ?? throw new NullReferenceException();
            string nickname = proj.Config.LocalConfig.Data.ProjectNickname;

            if (nickname != null && nickname != string.Empty)
                prefix = nickname;
        }
        
        // Calculate additional information and push to window title
        var debug = OS.IsDebugBuild() ? " DEBUG" : "";
        var version = string.Format("{0} b{1}.{2}",
            GitInfo.GitBranch(),
            GitInfo.GitCommitCountMainBranch(),
            GitInfo.GitCommitAhead()
        );

        DisplayServer.WindowSetTitle(string.Format("{0}: {1}   -   [{2}{3}]",
            prefix,
            app.AppTaskbarTitle,
            version,
            debug
        ));
    }
    private static void OnAppExited(AppScene app)
    {
        AppList.Remove(app);
    }

    #endregion
}