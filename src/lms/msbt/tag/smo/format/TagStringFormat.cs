using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

// ##################################################################### //
// ############ INCREDIBLY INCORRECT IMPLEMENTATION, PLZ FIX ########### //
// ##################################################################### //

public class MsbtTagElementStringFormat : MsbtTagElementWithTextData
{
    public MsbtTagElementStringFormat(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }

    public MsbtTagElementStringFormat(TagNameFormatString tag, string text)
        : base((ushort)TagGroup.FORMAT_STRING, (ushort)tag)
    {
        Text = text;
    }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize)
    {
        ReadTextData(ref pointer, buffer);
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        WriteTextData(value);

        return value.ToArray();
    }

    public override ushort CalcDataSize() { return base.CalcDataSize(); }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameFormatString), TagName))
            return Enum.GetName(typeof(TagNameFormatString), TagName);

        return "Unknown";
    }
};