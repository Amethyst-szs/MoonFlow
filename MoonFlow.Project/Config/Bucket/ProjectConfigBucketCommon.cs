using System;
using System.Linq;
using System.Text.Json.Serialization;

using static Nindot.RomfsPathUtility;

namespace MoonFlow.Project;

internal class ProjectConfigBucketCommon : IProjectFileFormatDataRoot
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

    private bool _isFirstBoot = true;
    [JsonInclude]
    internal bool IsFirstBoot
    {
        get { return _isFirstBoot; }
        set { _isFirstBoot = value; }
    }

    private bool _isDebugProject = false;
    [JsonInclude]
    internal bool IsDebugProject
    {
        get { return _isDebugProject; }
        set { _isDebugProject = value; }
    }

    #endregion

    #region Buckets

    [JsonInclude]
    internal ProjectConfigBucketEngineTarget Target = new();
    [JsonInclude]
    internal ProjectConfigBucketEventGraph EventGraph = new();

    #endregion
};