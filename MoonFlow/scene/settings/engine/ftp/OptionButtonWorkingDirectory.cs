using Godot;
using MoonFlow.Project.FTP;
using System;

namespace MoonFlow.Scene.Settings;

public partial class OptionButtonWorkingDirectory : OptionButton
{
    public enum OptionList
    {
        CUSTOM = 1000,

        ATMOSPHERE = 0,
        LUNAKIT = 1,
        QUICKMOON = 2,
    }

    [Export, ExportGroup("Internal References")]
    private LineEdit LineCustom;

    public override void _Ready()
    {
        var cur = ProjectFtpClient.CredentialStore.WorkingDirectory;
        
        LineCustom.Text = cur;
        LineCustom.Hide();

        switch (cur)
        {
            case ProjectFtpCredentialStore.TargetPresetAtmosphere:
                Selected = GetItemIndex((int)OptionList.ATMOSPHERE);
                break;
            case ProjectFtpCredentialStore.TargetPresetLunaKit:
                Selected = GetItemIndex((int)OptionList.LUNAKIT);
                break;
            case ProjectFtpCredentialStore.TargetPresetQuickMoon:
                Selected = GetItemIndex((int)OptionList.QUICKMOON);
                break;
            default:
                Selected = GetItemIndex((int)OptionList.CUSTOM);
                LineCustom.Show();
                break;
        }
    }

    private void OnItemSelected(int idx)
    {
        var id = (OptionList)GetItemId(idx);

        LineCustom.Hide();

        switch(id)
        {
            case OptionList.ATMOSPHERE:
                ProjectFtpClient.CredentialStore.SetTargetAtmosphere();
                break;
            case OptionList.LUNAKIT:
                ProjectFtpClient.CredentialStore.SetTargetLunaKit();
                break;
            case OptionList.QUICKMOON:
                ProjectFtpClient.CredentialStore.SetTargetQuickMoon();
                break;
            default:
                var cur = ProjectFtpClient.CredentialStore.WorkingDirectory;
                LineCustom.Text = cur;

                LineCustom.Show();
                break;
        }

        ProjectFtpClient.CredentialStore.Save();
    }

    private static void OnCustomTextChanged(string txt)
    {
        ProjectFtpClient.CredentialStore.SetTarget(txt);
    }

    private static void OnCustomTextSubmitted(string _)
    {
        ProjectFtpClient.CredentialStore.Save();
    }
}
