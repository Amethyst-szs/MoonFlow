using System;
using Godot;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.Scene.EditorMsbt;

[ScenePath("res://scene/editor/msbt/wheel/device/submenu_device.tscn")]
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