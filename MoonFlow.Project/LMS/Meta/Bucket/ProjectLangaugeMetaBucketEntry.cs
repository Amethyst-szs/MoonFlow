using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MoonFlow.Project;

public class ProjectLanguageMetaBucketEntry()
{
    [JsonInclude]
    public bool Mod = false;
    [JsonInclude]
    public bool OffSync = false;
    [JsonInclude]
    public bool Custom = false;

    public static readonly ProjectLanguageMetaBucketEntry Default = new();

    public bool IsModified() { return !Equals(Default); }
    public override bool Equals(object obj)
    {
        if (obj.GetType() != typeof(ProjectLanguageMetaBucketEntry))
            return false;

        var o = (ProjectLanguageMetaBucketEntry)obj;
        return Mod == o.Mod && OffSync == o.OffSync && Custom == o.Custom;
    }
    public override int GetHashCode() { return base.GetHashCode(); }
}