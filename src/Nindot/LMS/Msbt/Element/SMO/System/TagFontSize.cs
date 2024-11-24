using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementSystemFontSize : MsbtTagElementSystemBase
{
    // Converted to 100-based percentage in floating-point number format (NEON_ucvtf)
    public ushort FontSize = 100;

    public MsbtTagElementSystemFontSize(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementSystemFontSize(ushort fontSizePercentage)
        : base((ushort)TagGroup.System, (ushort)TagNameSystem.FontSize)
    {
        FontSize = fontSizePercentage;
    }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize)
    {
        FontSize = BitConverter.ToUInt16(buffer, pointer);
        pointer += sizeof(ushort);
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(FontSize);
        return value.ToArray();
    }

    public override ushort CalcDataSize() { return sizeof(ushort); }

    public override string GetTextureName() { return "System_FontSize"; }
};