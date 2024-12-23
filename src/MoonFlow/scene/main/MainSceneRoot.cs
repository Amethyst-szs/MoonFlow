using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using MoonFlow.Project;
using MoonFlow.Scene.Home;

namespace MoonFlow.Scene.Main;

public partial class MainSceneRoot : Control
{
    public Header NodeHeader = null;
    public Control NodeApps = null;
    public Taskbar NodeTaskbar = null;
    public VBoxContainer NodeAsync = null;

    public override void _Ready()
    {
        NodeHeader = GetNode<Header>("%Header");
        NodeApps = GetNode<Control>("%Apps");
        NodeTaskbar = GetNode<Taskbar>("%Taskbar");
        NodeAsync = GetNode<VBoxContainer>("%Async");

        // Add self-reference to ProjectManager
        ProjectManager.SceneRoot = this;
        TreeExiting += OnTreeExiting;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
            OnWindowCloseRequest();
    }

    private static void OnTreeExiting()
    {
        // Remove node reference in project manager
        ProjectManager.SceneRoot = null;
    }

    private async void OnWindowCloseRequest()
    {
        var awaiter = ToSignal(Engine.GetMainLoop(), "process_frame");

        foreach (var app in GetApps())
        {
            app.AppFocus();
            await awaiter;

            if (!app.TryCloseFromTreeQuit(out SignalAwaiter confirmationAwaiter))
            {
                if (confirmationAwaiter != null)
                    await confirmationAwaiter;

                if (IsInstanceValid(app) && !app.IsQueuedForDeletion())
                {
                    GD.Print("Tree could not quit because an application refused!");
                    return;
                }
            }
        }

        GetTree().Quit(0);
    }

    // ====================================================== //
    // ==================== App Utilities =================== //
    // ====================================================== //

    public IEnumerable<AppScene> GetApps()
    {
        return NodeApps.GetChildren().Cast<AppScene>();
    }

    // Returns first open app of type T
    public AppScene GetApp<T>()
    {
        foreach (var app in GetApps())
        {
            if (app.GetType() == typeof(T))
                return app;
        }

        return null;
    }

    public AppScene GetActiveApp()
    {
        foreach (var app in GetApps())
        {
            if (app.Visible)
                return app;
        }

        return null;
    }

    public void CloseActiveApp()
    {
        var app = GetActiveApp();
        if (app == null || !app.IsAppAllowUserToClose())
            return;

        app.AppClose(true);
    }
    public void ForceCloseAllApps()
    {
        foreach (var app in GetApps())
            app.AppClose(true);
    }
    public bool TryCloseAllApps()
    {
        foreach (var app in GetApps())
        {
            if (app is HomeRoot)
                continue;
            
            var res = app.AppClose(true);
            if (res != null)
            {
                app.AppFocus();
                return false;
            }
        }

        ForceCloseAllApps();
        return true;
    }

    public void FocusFirstApp()
    {
        var app = GetApps().First();
        var activeApp = GetActiveApp();

        if (app == activeApp || activeApp.IsAppExclusive())
            return;

        app.AppFocus();
    }
}
