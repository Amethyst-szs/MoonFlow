using Godot;
using System;

namespace MoonFlow.Scene.EditorWorld;

[SceneUid("uid://dwr02scc527sx")]
public partial class MapIcon : TextureRect
{
    [Export, ExportGroup("Internal References")]
    private AnimationPlayer Animation;

    public void SetStateFocus() => Animation.Play("focus");
    public void SetStateOtherFocused() => Animation.Play("other_focused");
    public void SetStateNothingFocused() => Animation.Play("nothing_focused");
}
