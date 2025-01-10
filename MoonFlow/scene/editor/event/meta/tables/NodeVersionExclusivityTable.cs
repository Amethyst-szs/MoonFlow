using System.Collections.Generic;

using static Nindot.RomfsPathUtility;

namespace MoonFlow.Scene.EditorEvent;

public static class MetaVersionExclusivityTable
{
    public static readonly Dictionary<string, RomfsVersion> Table = new(){
        { "VrGyroReset", RomfsVersion.v130 },
        { "CloseTalkMessageNoSe", RomfsVersion.v120 },
    };

    public static RomfsVersion Lookup(string type)
    {
        if (Table.TryGetValue(type, out RomfsVersion value))
            return value;

        return RomfsVersion.v100;
    }
}