using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementFormatting : MsbtTagElementWithTextData
{
    public ushort Unknown1 = 0;
    public ushort Unknown2 = 0;

    public MsbtTagElementFormatting(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        // Copy data from buffer at pointer
        Unknown1 = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;

        Unknown2 = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;

        ReadTextData(ref pointer, buffer);
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(Unknown1);
        value.Write(Unknown2);
        WriteTextData(value);

        return value.ToArray();
    }

    public override ushort GetDataSizeBase()
    {
        return 0x6;
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameFormatting), TagName))
            return Enum.GetName(typeof(TagNameFormatting), TagName);

        return "Unknown";
    }
};

public class MsbtTagElementFormattingSimple : MsbtTagElementWithTextData
{
    public MsbtTagElementFormattingSimple(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        ReadTextData(ref pointer, buffer);
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        WriteTextData(value);

        return value.ToArray();
    }

    public override ushort GetDataSizeBase()
    {
        return 0x2;
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameFormatting), TagName))
            return Enum.GetName(typeof(TagNameFormatting), TagName);

        return "Unknown";
    }
};