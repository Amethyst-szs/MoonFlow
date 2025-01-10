using Nindot.Al.SMO;
using static Nindot.RomfsPathUtility;

namespace MoonFlow.Project;

public struct ProjectInitInfo()
{
    public string Path = null;
    public RomfsVersion Version = RomfsVersion.INVALID_VERSION;
    public string DefaultLanguage = "USen";
}