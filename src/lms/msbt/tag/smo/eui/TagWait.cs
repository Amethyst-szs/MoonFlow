using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementEuiWait : MsbtTagElement
{
    public uint DelayFrames = 0;

    public MsbtTagElementEuiWait(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementEuiWait(uint frames)
        : base((ushort)TagGroup.Eui, (ushort)TagNameEui.Wait)
    {
        DelayFrames = frames;
    }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize)
    {
        DelayFrames = BitConverter.ToUInt32(buffer, pointer);
        pointer += sizeof(uint);
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(DelayFrames);
        return value.ToArray();
    }

    public override ushort CalcDataSize() { return sizeof(uint); }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameEui), TagName))
            return Enum.GetName(typeof(TagNameEui), TagName);

        return "Unknown";
    }
};