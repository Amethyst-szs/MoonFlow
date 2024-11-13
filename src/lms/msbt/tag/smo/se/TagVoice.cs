using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementVoice : MsbtTagElementWithTextData
{
    public MsbtTagElementVoice(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementVoice(string audio)
        : base((ushort)TagGroup.PLAY_SE, 0)
    {
        Text = audio;
    }

    public override string GetTagNameStr() { return "Voice"; }
};