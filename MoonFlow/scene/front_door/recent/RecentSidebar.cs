using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonFlow.Scene;

[SceneUid("uid://b00o2aortf3ba")]
public partial class RecentSidebar : VBoxContainer
{
    private List<string> History = [];
    private FrontDoor FrontDoorApp;

    [Export, ExportGroup("Internal References")]
    private VBoxContainer VBoxPanelHolder;

    private const int RecentProjectMaxCount = 12;
    private const string RecentProjectPath = "moonflow/general/proj_history";

    public override void _Ready()
    {
        // Locate parent front door app
        FrontDoorApp = this.FindParentByType<FrontDoor>();

        // Load history information
        History = [.. EngineSettings.GetSetting<string[]>(RecentProjectPath, Array.Empty<string>())];
        ClampHistoryLength();

        // Hide the recent sidebar if no recent projects exist
        if (History.Count == 0)
        {
            Hide();
            return;
        }

        // Clear children of recent project panel holder
        foreach (var child in VBoxPanelHolder.GetChildren())
            child.QueueFree();

        // Create panels for all recent projects
        foreach (var item in History)
        {
            var panel = SceneCreator<RecentEntryPanel>.Create();
            panel.SetupPanel(item);
            panel.Connect(RecentEntryPanel.SignalName.PanelPressed, Callable.From(
                new Action<string, bool>(OnPanelPressed)
            ));

            VBoxPanelHolder.AddChild(panel);
        }
    }

    #region Signals

    private void OnPanelPressed(string path, bool isDeleteFromHistory)
    {
        if (isDeleteFromHistory)
        {
            History.Remove(path);
            EngineSettings.SetSetting(RecentProjectPath, History.ToArray());
            EngineSettings.Save();

            _Ready();
            return;
        }

        FrontDoorApp.OnDialogOpenProjectPathSelected(path);
    }

    private void OnProjectOpened(string path)
    {
        History.Remove(path);
        History = [.. History.Prepend(path)];

        EngineSettings.SetSetting(RecentProjectPath, History.ToArray());
        EngineSettings.Save();

        _Ready();
    }

    #endregion

    #region Utilities

    private void ClampHistoryLength()
    {
        while (History.Count > RecentProjectMaxCount)
            History.RemoveAt(History.Count - 1);
    }

    #endregion
}
