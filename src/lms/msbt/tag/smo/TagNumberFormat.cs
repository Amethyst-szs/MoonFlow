using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementNumberFormat : MsbtTagElementWithTextData
{
    public ushort Figure = 0;
    public ushort IsJapaneseZenkaku = 0;

    public MsbtTagElementNumberFormat(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        // Copy data from buffer at pointer
        Figure = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;

        IsJapaneseZenkaku = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;

        ReadTextData(ref pointer, buffer);
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(Figure);
        value.Write(IsJapaneseZenkaku);
        WriteTextData(value);

        return value.ToArray();
    }

    public override ushort GetDataSizeBase()
    {
        return 0x6;
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameFormatNumber), TagName))
            return Enum.GetName(typeof(TagNameFormatNumber), TagName);

        return "Unknown";
    }
};

public class MsbtTagElementNumberFormatSimple : MsbtTagElementWithTextData
{
    public MsbtTagElementNumberFormatSimple(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
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
        if (Enum.IsDefined(typeof(TagNameFormatNumber), TagName))
            return Enum.GetName(typeof(TagNameFormatNumber), TagName);

        return "Unknown";
    }
};