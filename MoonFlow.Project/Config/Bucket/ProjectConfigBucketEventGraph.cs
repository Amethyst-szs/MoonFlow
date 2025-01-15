using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MoonFlow.Project;

internal class ProjectConfigBucketEventGraph
{
    private List<string> _pins = [];
    [JsonInclude]
    internal List<string> NodePins
    {
        get { return _pins; }
        set { _pins = value; }
    }
};