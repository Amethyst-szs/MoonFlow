using System.Collections.Generic;

using Nindot.LMS.Msbt.TagLib;
using System.IO;
using Nindot.LMS.Msbp;

namespace Nindot.LMS.Msbt;

public class MsbtEntry
{
    public MsbtFile Parent { get; private set; } = null;
    public List<MsbtBaseElement> Elements { get; private set; } = [];
    protected uint StyleIndex = 0xFFFFFFFF;

    public MsbtEntry(MsbtFile parent, byte[] txtData, uint styleIndex = 0xFFFFFFFF)
    {
        Parent = parent;
        StyleIndex = styleIndex;

        Elements = TagLibraryHolder.BuildMsbtElements(txtData, Parent);
    }

    public void WriteBytes(MemoryStream stream)
    {
        foreach (var item in Elements)
        {
            stream.Write(item.GetBytes());
        }
    }
}