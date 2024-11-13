using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementNumberFormat : MsbtTagElementWithTextData
{
    public ushort Figure = 0;
    public ushort IsJapaneseZenkaku = 0;

    public MsbtTagElementNumberFormat(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementNumberFormat(TagNameFormatNumber tagName)
        : base((ushort)TagGroup.FORMAT_NUMBER, (ushort)tagName)
    {
        Text = "";
    }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize)
    {
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

    public override ushort CalcDataSize() { return (ushort)(base.CalcDataSize() + sizeof(uint)); }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameFormatNumber), TagName))
            return Enum.GetName(typeof(TagNameFormatNumber), TagName);

        return "Unknown";
    }
};