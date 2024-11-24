using System;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementTimeComponent : MsbtTagElement
{
    public TagNameTime TimeComponent
    {
        get { return (TagNameTime)TagName; }
        set { TagName = (ushort)value; }
    }

    public MsbtTagElementTimeComponent(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementTimeComponent(TagNameTime timeComponent)
        : base((ushort)TagGroup.Time, (ushort)timeComponent) { }
    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize) { }

    public override ushort CalcDataSize() { return 0x0; }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameTime), TagName))
            return Enum.GetName(typeof(TagNameTime), TagName);

        return "Unknown";
    }

    public override string GetTextureName() { return "Time_" + GetTagNameStr(); }
};