using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementVoiceAudio : MsbtTagElementWithTextData
{
    public MsbtTagElementVoiceAudio(ref int pointer, byte[] buffer, MsbtFile parent) : base(ref pointer, buffer, parent)
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
        return "Voice";
    }
};