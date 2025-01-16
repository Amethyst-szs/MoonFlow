using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace MoonFlow.Scene;

public static class AppSceneServer
{
    #region Init

    private static Control AppRoot;
    private static List<AppScene> AppList = [];

    public static void Init(Control appRoot)
    {
        if (AppRoot != null)
            throw new Exception("AppSceneServer already has reference to app root");

        AppRoot = appRoot;

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

        app.AppFocus();
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
    }
    private static void OnAppExited(AppScene app)
    {
        AppList.Remove(app);
    }

    #endregion
}