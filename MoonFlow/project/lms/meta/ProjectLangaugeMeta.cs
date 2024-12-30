using System;
using System.Collections.Generic;

namespace MoonFlow.Project;

public class ProjectLanguageFileEntryMeta()
{
    public bool IsMod = false;
    public bool IsDisableSync = false;

    public static readonly ProjectLanguageFileEntryMeta Default = new();

    public bool IsModified() { return !Equals(Default); }
    public override bool Equals(object obj)
    {
        if (obj.GetType() != typeof(ProjectLanguageFileEntryMeta))
            return false;

        var o = (ProjectLanguageFileEntryMeta)obj;
        return IsMod == o.IsMod && IsDisableSync == o.IsDisableSync;
    }
    public override int GetHashCode() { return base.GetHashCode(); }
}

public class ProjectLanguageMetaData
{
    public Dictionary<string, ProjectLanguageFileEntryMeta> EntryTable = [];
    public Dictionary<string, long> TimeTable = [];
}