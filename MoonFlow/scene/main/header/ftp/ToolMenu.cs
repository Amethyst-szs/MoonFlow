using Godot;
using MoonFlow.Project.FTP;
using System;
using System.Threading.Tasks;

namespace MoonFlow.Scene.Main;

public partial class ToolMenu : Popup
{
    private async void OnPressButtonUploadProjectRomfs()
    {
        Hide();

        if (!await ProjectFtpClient.IsConnectedStill())
            return;
        
        GD.Print("no impl");
    }

    private async void OnPressButtonDeleteRemoteRomfs()
    {
        Hide();

        if (!await ProjectFtpClient.IsConnectedStill())
            return;
        
        GD.Print("no impl");
    }

    private async void OnPressButtonDeleteRemoteExefs()
    {
        Hide();

        if (!await ProjectFtpClient.IsConnectedStill())
            return;
        
        ProjectFtpClient.Delete(ProjectFtpClient.GetAtmosphereExefsPath(), true);
        ProjectFtpClient.Delete(ProjectFtpClient.GetAtmosphereExefsPatchesPath(), true);
    }
}
