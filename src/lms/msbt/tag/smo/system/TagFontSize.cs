using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementSystemFontSize : MsbtTagElementSystemCommon
{
    // Converted to 100-based percentage in floating-point number format (NEON_ucvtf)
    public ushort FontSize = 100;

    public MsbtTagElementSystemFontSize(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        if (!IsValid())
            return;

        // Copy short from buffer at pointer
        FontSize = BitConverter.ToUInt16(buffer, pointer);
        return;
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(FontSize);
        return value.ToArray();
    }

    public override ushort GetDataSizeBase()
    {
        return 0x2;
    }
};