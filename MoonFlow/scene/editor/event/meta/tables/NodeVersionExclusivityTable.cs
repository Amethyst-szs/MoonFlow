using System.Collections.Generic;

using Nindot.Al.SMO;

namespace MoonFlow.Scene.EditorEvent;

public static class MetaVersionExclusivityTable
{
    public static readonly Dictionary<string, RomfsValidation.RomfsVersion> Table = new(){
        { "VrGyroReset", RomfsValidation.RomfsVersion.v130 },
        { "CloseTalkMessageNoSe", RomfsValidation.RomfsVersion.v120 },
    };

    public static RomfsValidation.RomfsVersion Lookup(string type)
    {
        if (Table.TryGetValue(type, out RomfsValidation.RomfsVersion value))
            return value;

        return RomfsValidation.RomfsVersion.v100;
    }
}