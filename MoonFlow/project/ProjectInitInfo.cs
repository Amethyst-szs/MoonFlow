using Nindot.Al.SMO;

namespace MoonFlow.Project;

public struct ProjectInitInfo()
{
    public string Path = null;
    public RomfsValidation.RomfsVersion Version = RomfsValidation.RomfsVersion.INVALID_VERSION;
    public string DefaultLanguage = "USen";
}