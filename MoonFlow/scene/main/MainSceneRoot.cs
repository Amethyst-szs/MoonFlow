using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using MoonFlow.Project;
using MoonFlow.Scene.Home;
using System.Threading.Tasks;

namespace MoonFlow.Scene.Main;

public partial class MainSceneRoot : Control
{
    public Header NodeHeader = null;
    public Control NodeApps = null;
    public Taskbar NodeTaskbar = null;
    public VBoxContainer NodeAlerts = null;

    public override void _Ready()
    {
        NodeHeader = GetNode<Header>("%Header");
        NodeApps = GetNode<Control>("%Apps");
        NodeTaskbar = GetNode<Taskbar>("%Taskbar");
        NodeAlerts = GetNode<VBoxContainer>("%Alert");

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
        var isValidClose = await TryCloseAllApps();
        if (!isValidClose)
            return;
        
        // Update window properties in engine settings
        var win = GetWindow();
        var winSize = win.Size;
        const int winType = (int)Window.WindowInitialPosition.Absolute;

        if (win.Mode < Window.ModeEnum.Maximized)
        {
            EngineSettings.SetSetting("display/window/size/viewport_width", winSize.X);
            EngineSettings.SetSetting("display/window/size/viewport_height", winSize.Y);
            EngineSettings.SetSetting("display/window/size/initial_position_type", winType);
            EngineSettings.SetSetting("display/window/size/initial_position", win.Position);
        }
        else
        {
            EngineSettings.RemoveSetting("display/window/size/viewport_width");
            EngineSettings.RemoveSetting("display/window/size/viewport_height");
            EngineSettings.RemoveSetting("display/window/size/initial_position_type");
            EngineSettings.RemoveSetting("display/window/size/initial_position");
        }

        EngineSettings.SetSetting("display/window/size/mode", (int)win.Mode);
        EngineSettings.SetSetting("display/window/size/initial_screen", win.CurrentScreen);
        EngineSettings.Save();

        // Terminate application
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
    public async Task<bool> TryCloseAllApps()
    {
        var process_frame = ToSignal(Engine.GetMainLoop(), "process_frame");

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

    public void FocusFirstApp()
    {
        var app = GetApps().First();
        var activeApp = GetActiveApp();

        if (app == activeApp || activeApp.IsAppExclusive())
            return;

        app.AppFocus();
    }
}
