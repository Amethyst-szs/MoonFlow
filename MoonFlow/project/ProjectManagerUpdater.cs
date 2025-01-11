namespace MoonFlow.Project;

public static partial class ProjectManager
{
    public static void UpdateMsbpProjectSources()
    {
        var lang = GetMSBTArchives();
        var db = GetDB();
        if (lang == null || db == null)
            return;
        
        GetMSBPHolder().ReloadProjectSources(lang, db);
    }

    public static void UpdateMsbtLabelCache()
    {
        GetProject()?.MsgLabelCache?.UpdateCache();
    }
}