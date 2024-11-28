using Godot;
using System;

using Nindot.Al.SMO;

using MoonFlow.Fs.Rom;

namespace MoonFlow.Scene.Settings;

[Icon("res://asset/nindot/lms/icon/TextAnim_Wave.png")]
[ScenePath("res://scene/settings/romfs/romfs_accessor_config_app.tscn")]
public partial class RomfsAccessorConfigApp : AppScene
{
    private VBoxContainer PathError = null;

    protected override void AppInit()
    {
        PathError = GetNode<VBoxContainer>("%VBox_PathError");
        PathError.Hide();

        foreach (var version in Enum.GetNames(typeof(RomfsValidation.RomfsVersion)))
        {
            var versionEnum = Enum.Parse<RomfsValidation.RomfsVersion>(version);

            NodePath nodePath = string.Format("%{0}", version);
            if (!HasNode(nodePath))
                continue;

            var pathDisplay = GetNode<HBoxContainer>(nodePath);

            // Connect to path display signal
            if (!pathDisplay.HasSignal("delete_pressed"))
                throw new Exception("Path Display node missing delete signal!");

            pathDisplay.Connect("delete_pressed", Callable.From(new Action<string>(OnDeletePath)));

            // Update path display with path if available
            if (RomfsAccessor.VersionDirectories.TryGetValue(versionEnum, out string value))
                pathDisplay.Call("set_path", value);
            else
                pathDisplay.Call("set_no_path");
        }

        if (RomfsAccessor.VersionDirectories.Count == 0)
            ExitButtonHide();
        else
            ExitButtonShow();
    }

    private void OnPathPickerSelection(string dir)
    {
        RomfsAccessor.TryAssignDirectory(ref dir, out RomfsValidation.RomfsVersion version);
        if (version == RomfsValidation.RomfsVersion.INVALID_VERSION)
        {
            OnInvalidPathSelected();
            return;
        }

        PathError.Hide();
        ExitButtonShow();

        var pathDisplay = GetNode<HBoxContainer>(string.Format("%{0}", Enum.GetName(version)));
        pathDisplay.Call("set_path", dir);
    }

    private void OnDeletePath(string verStr)
    {
        if (!Enum.TryParse(verStr, out RomfsValidation.RomfsVersion ver))
            throw new Exception("Name doesn't exist in enum");
        
        RomfsAccessor.TryUnassignDirectory(ver);

        if (RomfsAccessor.VersionDirectories.Count == 0)
            ExitButtonHide();
        else
            ExitButtonShow();
    }

    private void OnInvalidPathSelected()
    {
        PathError.Show();
    }

    private void ExitButtonHide()
    {
        GetNode<Button>("%Button_CannotContinue").Show();
        GetNode<Button>("%Button_Save").Hide();
    }
    private void ExitButtonShow()
    {
        GetNode<Button>("%Button_Save").Show();
        GetNode<Button>("%Button_CannotContinue").Hide();
    }
}