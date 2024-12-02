using Nindot.Al.SMO;

namespace MoonFlow.Project;

public struct ProjectInitInfo()
{
    public string Path = null;
    public RomfsValidation.RomfsVersion Version = RomfsValidation.RomfsVersion.v100;
    public string DefaultLanguage = "USen";
}