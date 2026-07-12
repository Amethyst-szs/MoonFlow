using Godot;
using System;

using MoonFlow.Project;
using MoonFlow.Scene.EditorMsbt;

namespace MoonFlow.Scene.Home;

public partial class HBoxExtensionButtons : HBoxContainer
{
    [Export]
    private Godot.Collections.Dictionary<string, Control> ButtonMapping = [];

    public override void _Ready()
    {
        var config = ProjectManager.GetConfig();

        bool isAnyExtensionActive = false;
        foreach (var item in ButtonMapping)
        {
            if (!config.IsUseProjectExtension(item.Key))
            {
                item.Value.Hide();
                continue;
            }
            
            isAnyExtensionActive = true;
            item.Value.Show();
        }

        Visible = isAnyExtensionActive;
    }

    private static void OnExtensionButtonColorPaletteEditorPressed() { AppSceneServer.CreateApp<MsbpColorEditor>(); }
}
