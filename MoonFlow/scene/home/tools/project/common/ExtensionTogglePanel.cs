using Godot;
using System;

using MoonFlow.Project;

namespace MoonFlow.Home;

public partial class ExtensionTogglePanel : PanelContainer
{
    [Export]
    private string ExtensionId;

    [Export, ExportGroup("Visual")]
    private StyleBox PanelExtensionOn;
    [Export]
    private StyleBox PanelExtensionOff;

    [Export, ExportGroup("Internal References")]
    private CheckButton ToggleButton;

    public override void _Ready()
    {
        var config = ProjectManager.GetConfig();
        bool isActive = config.IsUseProjectExtension(ExtensionId);

        ToggleButton.SetPressedNoSignal(isActive);
        AddThemeStyleboxOverride("panel", isActive ? PanelExtensionOn : PanelExtensionOff);
    }

    private void OnToggleButtonToggleExtension(bool state)
    {
        var config = ProjectManager.GetConfig();
        if (!config.IsAcceptedExtensionWarning())
        {
            ToggleButton.SetPressedNoSignal(false);
            return;
        }

        config.SetUseProjectExtensionState(ExtensionId, state);
        config.WriteFile();

        AddThemeStyleboxOverride("panel", state ? PanelExtensionOn : PanelExtensionOff);
    }
}
