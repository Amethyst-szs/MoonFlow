using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementNumberWithFigure : MsbtTagElementWithTextData
{
    public ushort Figure = 0; // Number formatting style? Likely an enum
    public ushort IsJapaneseZenkaku = 0; // Is this number a full-width japanese character or a haf-width character?

    public string ReplacementKey
    {
        get { return Text; }
        set { Text = value; }
    }

    public MsbtTagElementNumberWithFigure(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementNumberWithFigure(TagNameNumber type, string replacementKey)
        : base((ushort)TagGroup.Number, (ushort)type)
    {
        ReplacementKey = replacementKey;

        if (type < TagNameNumber.Score || type > TagNameNumber.CoinNum)
            throw new Exception("Invalid tag name for MstTagElementNumberWithFigure");
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
        return TagName switch {
            (ushort)TagNameNumber.Score => "Score",
            (ushort)TagNameNumber.Fig02 => "Fig02",
            (ushort)TagNameNumber.CoinNum => "CoinNum",
            _ => throw new Exception("Invalid TagName"),
        };
    }

    public override string GetTextureName(int _)
    {
        return TagName switch {
            (ushort)TagNameNumber.Score => "Number_Score",
            (ushort)TagNameNumber.Fig02 => "Number_Score",
            (ushort)TagNameNumber.CoinNum => "Number_Coin",
            _ => throw new Exception("Invalid TagName"),
        };
    }
};