using System;

namespace MoonFlow.Project.FTP;

public struct ProjectFtpTargettingConfig()
{
    private string WorkingDirectory = "";

    public readonly string GetTarget()
    {
        if (WorkingDirectory == "")
        {
            Console.WriteLine("FTP: (WARNING) Targetting config has no path set, defaulting to atmosphere");
            return PresetAtmosphere;
        }

        return WorkingDirectory;
    }

    #region Presets

    public const string PresetAtmosphere = "/atmosphere/contents/0100000000010000/romfs/";
    public const string PresetLunaKit = "/LunaKit/";
    public const string PresetQuickMoon = "/switch/qm/";

    public void SetTarget(string path) { WorkingDirectory = path; }
    public void SetTargetAtmosphere() { WorkingDirectory = PresetAtmosphere; }
    public void SetTargetLunaKit() { WorkingDirectory = PresetLunaKit; }
    public void SetTargetQuickMoon() { WorkingDirectory = PresetQuickMoon; }

    #endregion
}