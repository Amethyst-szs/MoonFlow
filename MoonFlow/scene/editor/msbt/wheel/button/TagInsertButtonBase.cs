using Godot;

namespace MoonFlow.Scene.EditorMsbt;

[GlobalClass, Tool, Icon("res://asset/nindot/lms/icon/Number_Score.png")]
public partial class TagInsertButtonBase : Button
{
    [Export]
    public bool IsAutograbFocus = false;

    [Signal]
    public delegate void SelectedTagEventHandler(TagWheelTagResult tag);

    protected const string TexturePath = "res://asset/nindot/lms/icon/";

    public override void _Ready()
    {
        // Check if focus should be grabbed
        if (IsAutograbFocus)
            GrabFocus();
    }

    public override string[] _GetConfigurationWarnings()
    {
        if (GetType() == typeof(TagInsertButtonBase))
            return ["This node is a base class! Use inheriting child"];

        return [];
    }
}