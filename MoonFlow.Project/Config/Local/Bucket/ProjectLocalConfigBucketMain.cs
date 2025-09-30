using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace MoonFlow.Project;

public class ProjectLocalConfigBucketMain : IProjectFileFormatDataRoot
{
    #region Common

    [JsonInclude]
    public string CloneProjectUtilTargetPath = null;

    #endregion

    #region Buckets

    [JsonInclude]
    internal ProjectLocalConfigBucketEventGraph EventGraph = new();

    #endregion
};