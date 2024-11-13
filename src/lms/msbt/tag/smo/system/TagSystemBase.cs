using System;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementSystemCommon : MsbtTagElement
{
    public MsbtTagElementSystemCommon(ref int pointer, byte[] buffer) : base(ref pointer, buffer) {}

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameSystem), TagName))
            return Enum.GetName(typeof(TagNameSystem), TagName);

        return "Unknown";
    }
};