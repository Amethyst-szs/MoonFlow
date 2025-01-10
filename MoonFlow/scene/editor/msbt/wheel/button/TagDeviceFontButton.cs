using System;
using Godot;

using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.Scene.EditorMsbt;

[GlobalClass, Tool, Icon("res://asset/nindot/lms/icon/DeviceFont_ButtonA.png")]
public partial class TagDeviceFontButton : TagInsertButtonBase
{
    private TagNameDeviceFont _iconCode = TagNameDeviceFont.ButtonA;

    [Export]
    public TagNameDeviceFont IconCode
    {
        get { return _iconCode; }
        set
        {
            _iconCode = value;
            Icon = GetIconTexture(value);
            TooltipText = "DEVICE_FONT_TOOLTIP_" + Enum.GetName(value);
        }
    }

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _Pressed()
    {
        var tag = new MsbtTagElementDeviceFont(IconCode);
        EmitSignal(SignalName.SelectedTag, [new TagWheelTagResult(tag)]);
    }

    // ====================================================== //
    // ================ Editor Tool Utilities =============== //
    // ====================================================== //

    private static Texture2D GetIconTexture(TagNameDeviceFont code)
    {
        var tag = new MsbtTagElementDeviceFont(code);
        return GD.Load<Texture2D>(TexturePath + tag.GetTextureName(0) + ".png");
    }
}