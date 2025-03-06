using Godot;

namespace MoonFlow.Scene.EditorMsbt;

[SceneUid("uid://by1uqtvhxvu4b")]
public partial class TagSubmenuDeviceFont : TagSubmenuBase
{
    public override void InitSubmenu()
    {
        GetWindow().SizeChanged += OnWindowSizeChanged;
    }

    private void OnTagSelected(TagWheelTagResult tag)
    {
        CloseMenu(tag.Tag);
    }

    private void OnWindowSizeChanged()
    {
        SetupPosition(Vector2.Zero);
    }
}