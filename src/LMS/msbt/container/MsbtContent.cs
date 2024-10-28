using System.Collections.Generic;

namespace Nindot.LMS.Msbt;

public partial class Content : Dictionary<string, EntryContent>
{
    public Content(MessageStudio.Formats.BinaryText.Msbt file, TagLib.Core.Type tagLib)
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