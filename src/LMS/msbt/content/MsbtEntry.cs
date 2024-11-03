using System.Collections.Generic;

using Nindot.LMS.Msbt.TagLib;
using System.IO;
using Nindot.LMS.Msbp;

namespace Nindot.LMS.Msbt;

public class MsbtEntry
{
    public List<MsbtBaseElement> Elements { get; private set; } = [];
    protected uint StyleIndex = 0xFFFFFFFF;

    public MsbtEntry(TagLibraryHolder.Type tagLib, byte[] txtData, uint styleIndex = 0xFFFFFFFF)
    {
        StyleIndex = styleIndex;

        Elements = TagLibraryHolder.BuildMsbtElements(txtData, tagLib);
    }

    public byte[] GetBytes()
    {
        MemoryStream stream = new();

        foreach (var item in Elements)
        {
            stream.Write(item.GetBytes());
        }

        return stream.ToArray();
    }

    public uint GetStyleIndex()
    {
        return StyleIndex;
    }
}