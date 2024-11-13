using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

// Used for rendering Japanese Furigana
public class MsbtTagElementSystemPageBreak : MsbtTagElementWithTextData
{
    public MsbtTagElementSystemPageBreak(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementSystemPageBreak(ushort code, string content)
        : base((ushort)TagGroup.System, (ushort)TagNameSystem.PageBreak) { }
    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize) { }
};