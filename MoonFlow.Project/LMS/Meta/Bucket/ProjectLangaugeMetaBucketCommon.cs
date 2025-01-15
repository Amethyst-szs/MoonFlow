using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MoonFlow.Project;

internal class ProjectLanguageMetaBucketCommon : IProjectFileFormatDataRoot
{
    private Dictionary<string, ProjectLanguageMetaBucketEntry> _entryTable = [];
    [JsonInclude]
    internal Dictionary<string, ProjectLanguageMetaBucketEntry> EntryTable
    {
        get { return _entryTable; }
        set { _entryTable = value; }
    }

    private Dictionary<string, ProjectLanguageMetaBucketMsbtFile> _fileTable = [];
    [JsonInclude]
    internal Dictionary<string, ProjectLanguageMetaBucketMsbtFile> FileTable
    {
        get { return _fileTable; }
        set { _fileTable = value; }
    }
}