using System.Collections.Generic;

using Nindot.LMS.Msbt.TagLib;
using System.IO;

namespace Nindot.LMS.Msbt;

public class MsbtEntry
{
    public string Key;
    public List<MsbtBaseElement> ElementList;

    public MsbtEntry(string key, MsbtEntry entry)
    {
        // Setup raw values that require no parsing
        Key = key;

        return;
    }

    public byte[] BuildElementList()
    {
        MemoryStream stream = new();

        foreach (var item in ElementList)
        {
            stream.Write(item.GetBytes());
        }

        return stream.ToArray();
    }
}