using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.MsbtTagLibrary.Smo;
public class MsbtTagElementLanguageSpecial : MsbtTagElement
{
    public MsbtTagElementLanguageSpecial(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        if (TagName != 0)
            GD.Print("Language special has tag name of: ", TagName);
    }

    public override string GetTagNameStr()
    {
        return "Language Special";
    }
};