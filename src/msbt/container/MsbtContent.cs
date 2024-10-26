using MessageStudio.Formats.BinaryText;
using System.Collections.Generic;

namespace Nindot
{
    namespace MsbtContent
    {
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
        }
    }
}