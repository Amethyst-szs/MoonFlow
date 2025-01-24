global using static MoonFlow.Project.Global;

namespace MoonFlow.Project;

public static class Global
{
    #region Constants

    public static readonly string[] ProjectLanguageList = ["CNzh", "EUde", "EUen", "EUes", "EUfr", "EUit", "EUnl", "EUru", "JPja", "KRko", "TWzh", "USen", "USes", "USfr"];

    #endregion

    #region Configuration

    public static bool DebugConfigOutput { get; private set; } = false;
    public static bool DebugFsFtpLogging { get; set; } = false;

    public static void SetDebugMetadataFileOutput(bool enabled) => DebugConfigOutput = enabled;

    #endregion
}