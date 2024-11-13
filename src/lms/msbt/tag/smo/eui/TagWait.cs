using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementEuiWait : MsbtTagElement
{
    public ushort DelayFrames = 0;

    private enum SkipModeTable : ushort
    {
        NORMAL = 0,
        INPUT_REQUIRED = 1,
    };

    private ushort _skipMode;
    public ushort SkipMode
    {
        get { return _skipMode; }
        set
        {
            if (!Enum.IsDefined(typeof(SkipModeTable), value))
            {
#if !UNIT_TEST
                GD.PushWarning("Attempted to set Tag PrintDelay to invalid skip mode, clamped to 1");
#endif

                _skipMode = 1;
            }
            else
            {
                _skipMode = value;
            }
        }
    }

    public MsbtTagElementEuiWait(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementEuiWait(ushort frames)
        : base((ushort)TagGroup.EUI, (ushort)TagNameEui.WAIT)
    {
        DelayFrames = frames;
        SkipMode = 0;
    }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize)
    {
        DelayFrames = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;

        SkipMode = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(DelayFrames);
        value.Write(SkipMode);
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