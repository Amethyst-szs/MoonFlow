using Godot;
using System;

namespace MoonFlow.Scene.Settings;

[ScenePath("res://scene/settings/engine/engine_settings_app.tscn")]
[Icon("res://asset/app/icon/settings.png")]
public partial class EngineSettingsApp : AppScene
{
    public override void _ExitTree()
    {
        EngineSettings.Save();
    }

    private void OnSaveAndExit()
    {
        AppClose(true);
    }
}
