using Godot;
using System;

using MoonFlow.Async;
using MoonFlow.Project;
using MoonFlow.Project.Database;
using MoonFlow.Scene.EditorMsbt;

namespace MoonFlow.Scene.Home;

public partial class SubtabExtensions : VBoxContainer
{
    [Export, ExportGroup("Internal References")]
    private HFlowContainer ExtensionList;
    [Export]
    private VBoxContainer WarningAcceptButtonHolder;

    public override void _Ready()
    {
        UpdateVisiblePanels();
    }

    public void OnWarningAcceptButtonPressed()
    {
        var config = ProjectManager.GetConfig();
        config.SetAcceptedExtensionWarning(true);
        UpdateVisiblePanels();
    }

    private void UpdateVisiblePanels()
    {
        var config = ProjectManager.GetConfig();
        bool isAcceptedWarning = config.IsAcceptedExtensionWarning();

        ExtensionList.Visible = isAcceptedWarning;
        WarningAcceptButtonHolder.Visible = !isAcceptedWarning;
    }
}
