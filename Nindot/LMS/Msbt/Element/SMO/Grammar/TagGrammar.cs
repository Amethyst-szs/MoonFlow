using System;
using System.IO;

namespace Nindot.LMS.Msbt.TagLib.Smo;
public class MsbtTagElementGrammar : MsbtTagElement
{
    public TagNameGrammar Grammar
    {
        get { return (TagNameGrammar)TagName; }
        set { TagName = (ushort)value; }
    }

    // Some grammar tags in KRko langauge include additional data, more
    // research needed to understand their meaning/purpose
    private byte[] AdditionalData = [];

    public MsbtTagElementGrammar(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementGrammar(TagNameGrammar type)
        : base((ushort)TagGroup.Grammar, (ushort)type) { }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize)
    {
        AdditionalData = buffer[pointer..(pointer + dataSize)];
        pointer += dataSize;
    }

    public override ushort CalcDataSize() { return (ushort)AdditionalData.Length; }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(AdditionalData);
        return value.ToArray();
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameGrammar), TagName))
            return Enum.GetName(typeof(TagNameGrammar), TagName);

        return "Unknown";
    }

    public override string GetTextureName(int _)
    {
        return Grammar switch
        {
            TagNameGrammar.Cap => "Grammar_Cap",
            TagNameGrammar.Decap => "Grammar_Decap",
            _ => "Grammar_Unknown",
        };
    }
};