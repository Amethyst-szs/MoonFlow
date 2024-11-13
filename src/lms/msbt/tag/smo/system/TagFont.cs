using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementSystemFont : MsbtTagElementSystemCommon
{
    public TagFontIndex Font = TagFontIndex.MESSAGE_FONT;

    public MsbtTagElementSystemFont(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        // Copy data from buffer at pointer
        Font = (TagFontIndex)BitConverter.ToUInt16(buffer, pointer);
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(Font);
        return value.ToArray();
    }

    public override ushort GetDataSizeBase()
    {
        return 0x2;
    }
};