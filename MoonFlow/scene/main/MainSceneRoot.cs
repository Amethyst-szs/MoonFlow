using Godot;
using System.Collections.Generic;
using System.Linq;

using MoonFlow.Project;
using System.Threading.Tasks;
using MoonFlow.Project.FTP;

namespace MoonFlow.Scene.Main;

public partial class MainSceneRoot : Control
{
    public Header NodeHeader = null;
    public Control NodeApps = null;
    public Taskbar NodeTaskbar = null;
    public VBoxContainer NodeAlerts = null;

    public override void _Ready()
    {
        TreeExiting += OnTreeExiting;

        NodeHeader = GetNode<Header>("%Header");
        NodeApps = GetNode<Control>("%Apps");
        NodeTaskbar = GetNode<Taskbar>("%Taskbar");
        NodeAlerts = GetNode<VBoxContainer>("%Alert");

        // Initilize the ftp client now that we have access to header
        ProjectFtpClient.Init(NodeHeader.FtpStatusIndicator);
        
        ProjectManager.SceneRoot = this;
        AppSceneServer.Init(NodeApps);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
            OnWindowCloseRequest();
    }

    #region Signals

    private static void OnTreeExiting()
    {
        ProjectManager.SceneRoot = null;
        AppSceneServer.Destroy();
    }

    private async void OnWindowCloseRequest()
    {
        var isValidClose = await AppSceneServer.TryCloseAllApps();
        if (!isValidClose)
            return;
        
        // Update window properties in engine settings
        var win = GetWindow();
        var winSize = win.Size;
        const int winType = (int)Window.WindowInitialPosition.Absolute;

        EngineSettings.SetSetting("display/window/size/mode", (int)win.Mode);
        EngineSettings.SetSetting("display/window/size/initial_screen", win.CurrentScreen);

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

        EngineSettings.Save();

        // Terminate application
        GetTree().Quit(0);
    }

    private static void OnAppSceneServerWrapperCloseActive() { AppSceneServer.CloseActiveApp(); }
    private static void OnAppSceneServerWrapperFocusFirst() { AppSceneServer.FocusFirstApp(); }

    #endregion
}
