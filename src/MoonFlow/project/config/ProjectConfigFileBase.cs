using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;

using Nindot.LMS.Msbt;

using CsYaz0;

namespace MoonFlow.Project;

public abstract class ProjectConfigFileBase
{
    protected readonly string Path = null;
    protected static readonly JsonSerializerOptions JsonConfig = new()
    {
        IncludeFields = true,
        IgnoreReadOnlyFields = true,
    };

    // ====================================================== //
    // ==================== Init and Write ================== //
    // ====================================================== //

    public ProjectConfigFileBase(string path)
    {
        Path = path;
        if (!File.Exists(path))
            return;

        var data = File.ReadAllBytes(path);
        data = Yaz0.Decompress(data);

        var jsonStr = Encoding.UTF8.GetString(data);
        Init(jsonStr);
    }

    protected abstract void Init(string json);

    public bool WriteFile()
    {
        if (!TryGetWriteData(out object data))
            return false;

        string dataStr = JsonSerializer.Serialize(data, JsonConfig);
        byte[] bytes = Encoding.UTF8.GetBytes(dataStr);

        var dataCompressed = Yaz0.Compress(bytes);
        File.WriteAllBytes(Path, dataCompressed.ToArray());

        return true;
    }

    protected abstract bool TryGetWriteData(out object data);
}