using Godot;
using MoonFlow.Project;
using MoonFlow.Project.Database;
using System;

namespace MoonFlow.Scene.Home;

public partial class PanelGraphicsPreset : PanelContainer
{
    private static void OnButtonStartupPressed() => GraphicsPresetManager.InitManager(ProjectManager.GetPath());
    private static void OnButtonEndPressed() => GraphicsPresetManager.TryDestroyManager();
}
