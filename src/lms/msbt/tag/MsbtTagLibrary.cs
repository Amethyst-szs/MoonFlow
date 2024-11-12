using System.Collections.Generic;
using System.IO;

namespace Nindot.LMS.Msbt.TagLib;

public static class TagLibraryHolder
{
    public enum Type : ushort
    {
        NONE = 0,
        SUPER_MARIO_ODYSSEY = 1,
        ENUM_SIZE,
    }

    public readonly static string[] Name = [
        "None",
        "Super Mario Odyssey",
    ];

    public static List<MsbtBaseElement> BuildMsbtElements(byte[] buffer, Type tagLib)
    {
        // Call tag builder for selected tag library
        return tagLib switch {
            Type.SUPER_MARIO_ODYSSEY => Smo.Builder.Build(buffer),
            _ => BuildMsbtElementsWithoutTagLibrary(buffer),
        };
    }

    public static List<MsbtBaseElement> BuildMsbtElementsWithoutTagLibrary(byte[] buffer)
    {
        MsbtTextElement txt = new MsbtTextElement(buffer);
        txt.FinalizeAppending();

        List<MsbtBaseElement> list = [txt];
        return list;
    }
}