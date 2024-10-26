using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.MsbtTagLibrary.Smo;

public class MsbtTagElementShake : MsbtTagElement
{
    private enum ShakeTypeTable : ushort {
        SCARED_LETTER_SHAKE = 0,
        STRONG_ROTATION_SHAKE = 1,
        GENTLE_SWAY = 2,
        VERY_STRONG_SHAKE = 3,
        PULSE_TEXT_BOX = 4,
    };
    
    public ushort ShakeType
    {
        get { return TagName; }
        set
        {
            if (!Enum.IsDefined(typeof(ShakeTypeTable), value)) {
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
};