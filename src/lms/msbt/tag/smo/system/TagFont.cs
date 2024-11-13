using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementSystemFont : MsbtTagElementSystemCommon
{
    public TagFontIndex Font = TagFontIndex.MESSAGE_FONT;

    public MsbtTagElementSystemFont(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementSystemFont(TagFontIndex font)
        : base((ushort)TagGroup.SYSTEM, (ushort)TagNameSystem.FONT)
    {
        Font = font;
    }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize)
    {
        Font = (TagFontIndex)BitConverter.ToUInt16(buffer, pointer);
        pointer += sizeof(ushort);
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(Font);
        return value.ToArray();
    }

    public override ushort CalcDataSize() { return sizeof(ushort); }
};