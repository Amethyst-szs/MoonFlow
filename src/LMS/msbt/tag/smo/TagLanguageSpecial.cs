using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;
public class MsbtTagElementLanguageSpecial : MsbtTagElement
{
    public MsbtTagElementLanguageSpecial(ref int pointer, byte[] buffer, MsbtFile parent) : base(ref pointer, buffer, parent)
    {
        if (TagName != 0)
            GD.Print("Language special has tag name of: ", TagName);
    }

    public override string GetTagNameStr()
    {
        return "Language Special";
    }
};