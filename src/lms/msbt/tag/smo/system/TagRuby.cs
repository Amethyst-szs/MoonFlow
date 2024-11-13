using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

// Used for rendering Japanese Furigana
public class MsbtTagElementSystemRuby : MsbtTagElementWithTextData
{
    public ushort Code = 0;

    public MsbtTagElementSystemRuby(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        // Copy data from buffer at pointer
        Code = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;

        ReadTextData(ref pointer, buffer);
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(Code);
        WriteTextData(value);

        return value.ToArray();
    }

    public override ushort GetDataSizeBase()
    {
        return 0x4;
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameSystem), TagName))
            return Enum.GetName(typeof(TagNameSystem), TagName);

        return "Unknown";
    }
};