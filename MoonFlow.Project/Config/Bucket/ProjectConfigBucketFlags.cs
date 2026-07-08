using System;
using System.Linq;
using System.Text.Json.Serialization;

using static Nindot.RomfsPathUtility;

namespace MoonFlow.Project;

internal class ProjectConfigBucketFlags
{
    [JsonInclude]
    internal bool FirstBoot { get; set; } = true;
    [JsonInclude]
    internal bool AlwaysUpgrade { get; set; } = false;

    [JsonInclude]
    internal bool DebugProject { get; set; } = false;
};