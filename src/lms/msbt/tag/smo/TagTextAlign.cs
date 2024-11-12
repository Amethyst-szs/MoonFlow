using System;
using Godot;

namespace Nindot.LMS.Msbt.TagLib.Smo;
public class MsbtTagElementTextAlign : MsbtTagElement
{
    public TagNameTextAlign TextAlignment
    {
        get { return (TagNameTextAlign)TagName; }
        set
        {
            if (!Enum.IsDefined(typeof(TagNameTextAlign), value))
            {
                GD.PushWarning("Attempted to set Tag TextAlign to invalid alignment, set to default value instead");
                TagName = (ushort)TagNameTextAlign.LEFT;
            }
            else
            {
                TagName = (ushort)value;
            }
        }
    }

    public MsbtTagElementTextAlign(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        TextAlignment = (TagNameTextAlign)TagName;
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameTextAlign), TagName))
            return Enum.GetName(typeof(TagNameTextAlign), TagName);

        return "Unknown";
    }
};