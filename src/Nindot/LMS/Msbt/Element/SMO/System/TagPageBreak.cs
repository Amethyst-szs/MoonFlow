namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementSystemPageBreak : MsbtTagElementSystemBase
{
    public MsbtTagElementSystemPageBreak(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementSystemPageBreak()
        : base((ushort)TagGroup.System, (ushort)TagNameSystem.PageBreak) { }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize) { }

    public override bool IsPageBreak() { return true; }
};