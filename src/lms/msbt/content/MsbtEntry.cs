using System.Collections.Generic;

using Nindot.LMS.Msbt.TagLib;
using System.IO;
using Nindot.LMS.Msbp;

namespace Nindot.LMS.Msbt;

public class MsbtEntry
{
    protected readonly TagLibraryHolder.Type TagLibrary;

    public List<MsbtBaseElement> Elements { get; private set; } = [];
    protected uint StyleIndex = 0xFFFFFFFF;

    internal MsbtEntry(TagLibraryHolder.Type tagLib, byte[] txtData, uint styleIndex = 0xFFFFFFFF)
    {
        TagLibrary = tagLib;
        Elements = TagLibraryHolder.BuildMsbtElements(txtData, tagLib);
        StyleIndex = styleIndex;
    }
    public MsbtEntry(TagLibraryHolder.Type tagLib)
    {
        TagLibrary = tagLib;
        Elements.Add(new MsbtTextElement(""));
    }
    public MsbtEntry(TagLibraryHolder.Type tagLib, string textContent)
    {
        TagLibrary = tagLib;
        Elements.Add(new MsbtTextElement(textContent));
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