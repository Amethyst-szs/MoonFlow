using System;

namespace Nindot.LMS.Msbt.TagLib.Smo;
public class MsbtTagElementTextAlign : MsbtTagElement
{
    public TagNameTextAlign TextAlignment
    {
        get { return (TagNameTextAlign)TagName; }
        set { TagName = (ushort)value; }
    }

    public MsbtTagElementTextAlign(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementTextAlign(TagNameTextAlign type)
        : base((ushort)TagGroup.TextAlign, (ushort)type) { }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize) { }

    public override ushort CalcDataSize() { return 0x0; }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameTextAlign), TagName))
            return Enum.GetName(typeof(TagNameTextAlign), TagName);

        return "Unknown";
    }

    public override string GetTextureName() {
        return TextAlignment switch
        {
            TagNameTextAlign.Left => "TextAlign_Left",
            TagNameTextAlign.Center => "TextAlign_Center",
            _ => null,
        };
    }
};