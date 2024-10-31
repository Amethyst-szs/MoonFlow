using System;
using Godot;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementTime : MsbtTagElement
{
    public TagNameTime TimeType
    {
        get { return (TagNameTime)TagName; }
        set
        {
            if (!Enum.IsDefined(typeof(TagNameTime), value))
            {
#if !UNIT_TEST
                GD.PushWarning("Attempted to set Tag TimeType to invalid type, value clamped to 6");
#endif

                TagName = 0x6;
            }
            else
            {
                TagName = (ushort)value;
            }
        }
    }

    public MsbtTagElementTime(ref int pointer, byte[] buffer, MsbtFile parent) : base(ref pointer, buffer, parent)
    {
        if (!IsValid())
            return;

        // The tag name is the time type, so make sure to assign it to itself here to run the enum clamper
        TimeType = (TagNameTime)TagName;
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameTime), TagName))
            return Enum.GetName(typeof(TagNameTime), TagName);

        return "Unknown";
    }
};