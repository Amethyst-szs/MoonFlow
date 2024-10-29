using System.Collections.Generic;
using System.IO;

namespace Nindot.LMS.Msbt.TagLib;

public static class Core
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

    public static List<MsbtBaseElement> BuildElementList(byte[] buffer, Type type)
    {
        // Call tag builder for selected tag library
        switch (type)
        {
            case Type.SUPER_MARIO_ODYSSEY:
                return Smo.Builder.Build(buffer);
            default:
                MsbtTextElement txt = new MsbtTextElement(buffer);
                txt.FinalizeAppending();

                List<MsbtBaseElement> list = [txt];
                return list;
        }
    }

    public static byte[] ParseElementListIntoBuffer(EntryContent entry)
    {
        MemoryStream buffer = new MemoryStream();

        foreach (MsbtBaseElement element in entry.ElementList)
        {
            buffer.Write(element.GetBytes());
        }

        return buffer.ToArray();
    }
}