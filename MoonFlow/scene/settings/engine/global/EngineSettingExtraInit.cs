using MoonFlow.Project;

namespace MoonFlow.Scene.Settings;

public static class EngineSettingExtraInit
{
    [StartupTask]
    public static void Init()
    {
        // Setup MoonFlow.Project globals from EngineSettings
        Global.DebugFsFtpLogging = EngineSettings.GetSetting<bool>("moonflow/debug/ftp_logging", false);
    }
}