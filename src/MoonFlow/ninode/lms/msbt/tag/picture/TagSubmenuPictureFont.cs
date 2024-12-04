using System;
using Godot;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.LMS.Msbt;

[ScenePath("res://ninode/lms/msbt/tag/picture/submenu_pict.tscn")]
public partial class TagSubmenuPictureFont : TagSubmenuBase
{
    public override void InitSubmenu()
    {
        foreach (var type in Enum.GetValues<TagNamePictureFont>())
        {
            var button = new TagPictureFontButton(type);
            GetNode<HBoxContainer>("%HBox").AddChild(button);
        }

        (GetNode<HBoxContainer>("%HBox").GetChild(0) as Control).GrabFocus();
    }
}