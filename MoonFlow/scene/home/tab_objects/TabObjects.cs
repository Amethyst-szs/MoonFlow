using Godot;
using System;

namespace MoonFlow.Scene.Home;

[SceneUid("uid://cvdgsq1ltxug2")]
public partial class TabObjects : Control
{
    public string SelectedArc { get; private set; } = null;
}
