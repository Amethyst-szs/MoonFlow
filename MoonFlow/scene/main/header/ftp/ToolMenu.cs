using Godot;
using MoonFlow.Project;
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

        if (!ProjectManager.IsProjectExist())
            return;

        ProjectFtpClient.UploadProjectAll(ProjectManager.GetPath());
    }

    private async void OnPressButtonDeleteRemoteRomfs()
    {
        Hide();

        if (!await ProjectFtpClient.IsConnectedStill())
            return;

        ProjectFtpClient.Delete(ProjectFtpClient.CredentialStore.WorkingDirectory, true, true);
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
