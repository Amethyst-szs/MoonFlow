using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonFlow.Scene;

[SceneUid("uid://b00o2aortf3ba")]
public partial class RecentSidebar : VBoxContainer
{
    private List<string> History = [];

    [Export, ExportGroup("Internal References")]
    private VBoxContainer VBoxPanelHolder;

    private const int RecentProjectMaxCount = 12;
    private const string RecentProjectPath = "moonflow/general/proj_history";

    public override void _Ready()
    {
        // Load history information
        History = [.. EngineSettings.GetSetting<string[]>(RecentProjectPath, Array.Empty<string>())];
        ClampHistoryLength();

        // Delete the recent sidebar if no recent projects exist
        if (History.Count == 0)
        {
            QueueFree();
            return;
        }

        // Create panels for all recent projects
        foreach (var item in History)
        {
            for (int i = 0; i < 2; i++) // Temp debug code just for testing
            {
                var panel = SceneCreator<RecentEntryPanel>.Create();
                panel.SetupPanel(item);
                panel.Connect(RecentEntryPanel.SignalName.PanelPressed, Callable.From(
                    new Action<string>(OnPanelPressed)
                ));

                VBoxPanelHolder.AddChild(panel);
            }
        }
    }

    #region Signals

    private void OnPanelPressed(string path)
    {
        GD.Print("Pressed " + path);
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
