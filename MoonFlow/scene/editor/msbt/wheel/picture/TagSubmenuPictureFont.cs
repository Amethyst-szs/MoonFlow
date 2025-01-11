using Godot;

namespace MoonFlow.Scene.EditorMsbt;

[ScenePath("res://scene/editor/msbt/wheel/picture/submenu_pict.tscn")]
public partial class TagSubmenuPictureFont : TagSubmenuBase
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