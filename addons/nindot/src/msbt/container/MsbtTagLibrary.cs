using System.Collections.Generic;
using System.IO;
using Nindot.MsbtContent;

namespace Nindot
{
    namespace MsbtTagLibrary
    {
        public struct Core
        {
            public enum Type : ushort
            {
                NONE = 0,
                SUPER_MARIO_ODYSSEY = 1,
                ENUM_SIZE,
            }

            public static string[] Name = [
                "None",
                "Super Mario Odyssey",
            ];

            public static List<MsbtBaseElement> TextElementsBuildList(byte[] buffer, Type type)
            {
                List<MsbtBaseElement> list = new List<MsbtBaseElement>();

                // Call tag builder for selected tag library
                switch (type)
                {
                    case Type.SUPER_MARIO_ODYSSEY:
                        return [];
                    default:
                        list.Add(new MsbtTextElement(buffer));
                        return list;
                }
            }

            public static byte[] TextElementsParseIntoBuffer(EntryContent entry)
            {
                MemoryStream buffer = new MemoryStream();

                foreach (MsbtBaseElement element in entry.TextTagList)
                {
                    buffer.Write(element.GetBytes());
                }

                return buffer.ToArray();
            }
        }
    }
}