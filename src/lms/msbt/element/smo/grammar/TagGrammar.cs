using System;
using Godot;

namespace Nindot.LMS.Msbt.TagLib.Smo;
public class MsbtTagElementGrammar : MsbtTagElement
{
    public TagNameGrammar Grammar
    {
        get { return (TagNameGrammar)TagName; }
        set { TagName = (ushort)value; }
    }

    public MsbtTagElementGrammar(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementGrammar(TagNameGrammar type)
        : base((ushort)TagGroup.Grammar, (ushort)type) { }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize) { }

    public override ushort CalcDataSize() { return 0x0; }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameGrammar), TagName))
            return Enum.GetName(typeof(TagNameGrammar), TagName);

        return "Unknown";
    }
};