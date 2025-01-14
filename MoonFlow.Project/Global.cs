global using static MoonFlow.Project.Global;

namespace MoonFlow.Project;

public static class Global
{
    public static bool DebugConfigOutput { get; private set; } = false;

    public static void SetDebugMetadataFileOutput(bool enabled)
    {
        DebugConfigOutput = enabled;
    }
}