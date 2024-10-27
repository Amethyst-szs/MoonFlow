using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.MsbtTagLibrary.Smo;

public class MsbtTagElementShake : MsbtTagElement
{
    public ushort ShakeType
    {
        get { return TagName; }
        set
        {
            if (!Enum.IsDefined(typeof(TagNameShakeAnimator), value)) {
                #if !UNIT_TEST
                GD.PushWarning("Attempted to set Tag ShakeType to invalid type, clamped to 4");
                #endif
                
                TagName = 0x4;
            } else {
                TagName = value;
            }
        }
    }

    public MsbtTagElementShake(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        if (!IsValid())
            return;
        
        // The tag name is the shake type, so make sure to assign it to itself here to run the enum clamper
        ShakeType = TagName;
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameShakeAnimator), TagName))
            return Enum.GetName(typeof(TagNameShakeAnimator), TagName);
        
        return "Unknown";
    }
};