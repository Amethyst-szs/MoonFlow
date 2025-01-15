using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MoonFlow.Project;

internal class ProjectConfigBucketEngineTarget
{
    private const string ExceptionMessage = "Don't attempt to fetch values from EngineTarget bucket before init!";

    private string _name;
    [JsonInclude]
    internal string Name
    {
        get
        {
            if (_name == null)
                throw new NullReferenceException(ExceptionMessage);

            return _name;
        }
        set { _name = value; }
    }

    private string _commitHash;
    [JsonInclude]
    internal string CommitHash
    {
        get
        {
            if (_commitHash == null)
                throw new NullReferenceException(ExceptionMessage);
            
            if (_commitHash.Length != 40)
                throw new Exception("Commit hash is an invalid length");

            return _commitHash;
        }
        set
        {
            if (value.Length != 40)
                throw new Exception("Commit hash is an invalid length");

            _commitHash = value;
        }
    }

    private long _unixTime = 0;
    [JsonInclude]
    internal long UnixTime
    {
        get
        {
            if (_unixTime == 0)
                throw new Exception(ExceptionMessage);

            return _unixTime;
        }
        set
        {
            _unixTime = value;
        }
    }
};