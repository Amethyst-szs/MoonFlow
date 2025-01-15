using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MoonFlow.Project;

public class ProjectLanguageMetaBucketMsbtFile()
{
    [JsonInclude]
    public long UnixTime = DateTime.UnixEpoch.ToFileTimeUtc();

    public static readonly ProjectLanguageMetaBucketEntry Default = new();

    public bool IsModified() { return !Equals(Default); }
    public override bool Equals(object obj)
    {
        if (obj.GetType() != typeof(ProjectLanguageMetaBucketMsbtFile))
            return false;

        var o = (ProjectLanguageMetaBucketMsbtFile)obj;
        return UnixTime == o.UnixTime;
    }
    public override int GetHashCode() { return base.GetHashCode(); }
}