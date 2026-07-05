using Godot;
using System;

namespace MoonFlow.Scene.EditorMsbt;

[SceneUid("uid://c12e0fj0nmls8")]
public partial class WindowMassAddLabels : TagEditScene
{
    [Signal]
    public delegate void TextSubmittedEventHandler(string fullTxt);

    public void _EmitSignalTextSubmitted(string txt) { EmitSignalTextSubmitted(txt); }
}
