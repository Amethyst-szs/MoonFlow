using System;
using System.Linq;
using System.Text.Json.Serialization;

using static Nindot.RomfsPathUtility;

namespace MoonFlow.Project;

public class ProjectConfigBucketCommon : IProjectFileFormatDataRoot
{
    #region Common

    private RomfsVersion _version = RomfsVersion.INVALID_VERSION;
    [JsonInclude]
    internal RomfsVersion Version
    {
        get { return _version; }
        set
        {
            if (!Enum.IsDefined(value))
            {
                _version = RomfsVersion.INVALID_VERSION;
                return;
            }

            _version = value;
        }
    }

    private string _defaultLanguage = "USen";
    [JsonInclude]
    internal string DefaultLanguage
    {
        get { return _defaultLanguage; }
        set
        {
            if (ProjectLanguageList.Contains(value))
                _defaultLanguage = value;
        }
    }

    #endregion

    #region Buckets

    [JsonInclude]
    internal ProjectConfigBucketFlags Flags = new();
    [JsonInclude]
    internal ProjectConfigBucketEngineTarget Target = new();
    [JsonInclude]
    internal ProjectConfigBucketEventGraph EventGraph = new();

    #endregion
};