using Godot;
using System;

using MoonFlow.Scene.EditorMsbt;

namespace MoonFlow.Scene.Home;

public partial class TabProject : HSplitContainer
{
    private static void OnOpenMsbpColorEditor() { AppSceneServer.CreateApp<MsbpColorEditor>(); }
}
