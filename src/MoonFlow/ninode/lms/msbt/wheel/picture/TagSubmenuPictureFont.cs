using System;
using Godot;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.LMS.Msbt;

[ScenePath("res://ninode/lms/msbt/wheel/picture/submenu_pict.tscn")]
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