using Godot;
using MoonFlow.Project.FTP;
using System;

namespace MoonFlow.Scene.Settings;

public partial class CheckSyncAllLangauges : CheckBox
{
    public override void _Ready()
    {
        bool isAll = ProjectFtpClient.CredentialStore.IsTransferAllLanguages;
        SetPressedNoSignal(isAll);
    }

    public override void _Toggled(bool toggledOn)
    {
        ProjectFtpClient.CredentialStore.IsTransferAllLanguages = toggledOn;
        ProjectFtpClient.CredentialStore.Save();
    }
}
