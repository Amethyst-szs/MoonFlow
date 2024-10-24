using Godot;
using System.Collections.Generic;

using MessageStudio.Formats.BinaryText;

namespace Nindot
{
    namespace MsbtContent
    {
        public struct EntryContent
        {
            public string Key;

            public MsbtTagLibrary.Core.Type TagLibrary;

            public string TextRaw;

            public List<MsbtBaseElement> TextTagList;

            public EntryContent(string key, MsbtEntry entry, MsbtTagLibrary.Core.Type tagLib)
            {
                // Setup raw values that require no parsing
                Key = key;
                TagLibrary = tagLib;
                TextRaw = entry.Text;

                // Build TextElement array
                TextTagList = MsbtTagLibrary.Core.TextElementsBuildList(entry.Text.ToUtf16Buffer(), TagLibrary);
            }
        }
    }
}