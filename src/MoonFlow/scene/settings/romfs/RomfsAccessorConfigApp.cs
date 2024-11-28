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

    public override void _Ready()
    {
        base._Ready();

        PathError = GetNode<VBoxContainer>("%VBox_PathError");
        PathError.Hide();

        foreach (var version in Enum.GetNames(typeof(RomfsValidation.RomfsVersion)))
        {
            var versionEnum = Enum.Parse<RomfsValidation.RomfsVersion>(version);

            NodePath nodePath = string.Format("%{0}", version);
            if (!HasNode(nodePath))
                continue;

            var pathDisplay = GetNode<HBoxContainer>(nodePath);
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