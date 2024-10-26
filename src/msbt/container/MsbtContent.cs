using MessageStudio.Formats.BinaryText;
using System.Collections.Generic;
using System.Linq;

namespace Nindot.MsbtContent;

public partial class Content : Dictionary<string, EntryContent>
{
    public Content(Msbt file, MsbtTagLibrary.Core.Type tagLib)
    {
        // Iterate through every item in the Msbt class and create an MsbtDictionaryEntry
        foreach (var entry in file)
        {
            var element = new EntryContent(entry.Key, entry.Value, tagLib);
            Add(entry.Key, element);
        }
    }

    public Dictionary<string, KeyValuePair<byte[], string>> GenerateToBinaryDictionary()
    {
        Dictionary<string, KeyValuePair<byte[], string>> build = [];

        foreach (var item in Values)
        {
            KeyValuePair<byte[], string> pair = new(item.BuildElementList(), item.Attribute);
            build.Add(item.Key, pair);
        }

        return build;
    }
}