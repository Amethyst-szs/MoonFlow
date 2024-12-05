using Godot;
using Nindot.LMS.Msbt.TagLib.Smo;

using MoonFlow.Project;
using Godot.Collections;
using System;
using Nindot.Al.SMO;

namespace MoonFlow.LMS.Msbt;

[GlobalClass, Tool, Icon("res://asset/nindot/lms/icon/Number_Score.png")]
public partial class TagPictureFontButton : Button
{
    private IconCodePictureFont _iconCode = IconCodePictureFont.GlyphDot;

    [Export]
    public IconCodePictureFont IconCode
    {
        get { return _iconCode; }
        set
        {
            _iconCode = value;

            if (value != IconCodePictureFont.GlyphColon_IconBalloonHintArrow)
            {
                UpdateVersionRequirement(value);
                Icon = GetIconTexture(value);
                TooltipText = "PICTURE_FONT_TOOLTIP_" + Enum.GetName(value) + "_" + VersionRequirement;
            }
        }
    }


    [Export(PropertyHint.Enum, "Any Version:0,Before 1.2.0:1,After 1.2.0:2")]
    public int VersionRequirement;

    [Export]
    public bool IsAutograbFocus = false;

    [Signal]
    public delegate void SelectedTagEventHandler(TagWheelTagResult tag);

    private const string TexturePath = "res://asset/nindot/lms/icon/";

    public override void _Ready()
    {
        // Check if focus should be grabbed
        if (IsAutograbFocus)
            GrabFocus();

        // Ensure this node is allowed to exist given the current project version
        var proj = ProjectManager.GetProject();
        if (proj == null)
            return;

        var ver = (int)proj.Config.Version;
        if (VersionRequirement == 1 && ver >= (int)RomfsValidation.RomfsVersion.v120)
            QueueFree();

        if (VersionRequirement == 2 && ver < (int)RomfsValidation.RomfsVersion.v120)
            QueueFree();
    }

    public override void _Pressed()
    {
        var tag = new MsbtTagElementPictureFont(IconCode);
        EmitSignal(SignalName.SelectedTag, [new TagWheelTagResult(tag)]);
    }

    // ====================================================== //
    // ================ Editor Tool Utilities =============== //
    // ====================================================== //

    private void UpdateVersionRequirement(IconCodePictureFont code)
    {
        if (code == IconCodePictureFont.IconStarSmall)
        {
            VersionRequirement = 2;
            return;
        }

        VersionRequirement = 0;
    }

    private Texture2D GetIconTexture(IconCodePictureFont code)
    {
        var tag = new MsbtTagElementPictureFont(code);
        return GD.Load<Texture2D>(TexturePath + tag.GetTextureName(100) + ".png");
    }
}