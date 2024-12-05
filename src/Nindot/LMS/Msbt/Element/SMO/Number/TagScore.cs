using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementNumberScore : MsbtTagElementWithTextData
{
    // Need to do some research on what these properties actually mean
    public ushort Figure = 0;
    public ushort IsJapaneseZenkaku = 0;

    public string ReplacementKey
    {
        get { return Text; }
        set { Text = value; }
    }

    public MsbtTagElementNumberScore(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementNumberScore(string replacementKey)
        : base((ushort)TagGroup.Number, (ushort)TagNameNumber.Score)
    {
        ReplacementKey = replacementKey;
    }
    internal MsbtTagElementNumberScore(string replacementKey, TagNameNumber tag)
        : base((ushort)TagGroup.Number, (ushort)tag)
    {
        ReplacementKey = replacementKey;
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
    public override string GetTagNameStr() { return "Score"; }

    public override string GetTextureName(int _) { return "Number_Score"; }
};