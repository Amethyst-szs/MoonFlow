using Godot;
using System;

namespace MoonFlow.Scene.EditorWorld;

[SceneUid("uid://crlg0ndcpvyv5")]
public partial class StageTypeSeparator : HBoxContainer
{
    [Export]
    private Label LabelName;

    public StageTypeSeparator Init(string name)
    {
        LabelName.Text = name;
        return this;
    }
}
