using Godot;
using System.Collections.Generic;

using MessageStudio.Formats.BinaryText;

using Nindot.MsbtTagLibrary;
using System.IO;

namespace Nindot
{
    namespace MsbtContent
    {
        public struct EntryContent
        {
            public string Key;

            public Core.Type TagLibrary;

            public string Attribute;

            public List<MsbtBaseElement> ElementList;

            public EntryContent(string key, MsbtEntry entry, Core.Type tagLib)
            {
                // Setup raw values that require no parsing
                Key = key;
                TagLibrary = tagLib;
                Attribute = entry.Attribute;

                // Build ElementList
                ElementList = Core.BuildElementList(entry.TextBuffer, TagLibrary);

                return;
            }

            public readonly byte[] BuildElementList()
            {
                MemoryStream stream = new();

                foreach (var item in ElementList)
                {
                    stream.Write(item.GetBytes());
                }

                return stream.ToArray();
            }
        }
    }
}