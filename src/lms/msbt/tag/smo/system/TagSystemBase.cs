using System;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public abstract class MsbtTagElementSystemCommon : MsbtTagElement
{
    public MsbtTagElementSystemCommon(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementSystemCommon(ushort group, ushort tag) : base(group, tag) { }
    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize) { }
    public override ushort CalcDataSize() { return 0; }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameSystem), TagName))
            return Enum.GetName(typeof(TagNameSystem), TagName);

        return "Unknown";
    }
};