using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;
using Godot;

using CsYaz0;

namespace MoonFlow.Project;

public abstract class ProjectFileFormatBase
{
    protected string Path = null;
    public bool IsReadFromDisk { get; private set; } = false;

    protected readonly JsonSerializerOptions JsonConfig = new();

    // ====================================================== //
    // ==================== Init and Write ================== //
    // ====================================================== //

    public ProjectFileFormatBase(bool isStoreColorAlpha = false)
    {
        AddJsonConverters(isStoreColorAlpha);
        IsReadFromDisk = true;
    }

    public ProjectFileFormatBase(string path, bool isStoreColorAlpha = false)
    {
        AddJsonConverters(isStoreColorAlpha);

        Path = path;
        if (!File.Exists(path))
            return;

        var data = File.ReadAllBytes(path);
        data = Yaz0.Decompress(data);

        var jsonStr = Encoding.UTF8.GetString(data);
        Init(jsonStr);

        IsReadFromDisk = true;
    }

    public ProjectFileFormatBase(byte[] data, bool isStoreColorAlpha = false)
    {
        AddJsonConverters(isStoreColorAlpha);

        data = Yaz0.Decompress(data);

        var jsonStr = Encoding.UTF8.GetString(data);
        Init(jsonStr);

        IsReadFromDisk = true;
    }

    protected abstract void Init(string json);

    public void ChangeWritePath(string path) { Path = path; }
    public bool WriteFile()
    {
        if (!TryGetWriteData(out object data))
            return false;

        string dataStr = JsonSerializer.Serialize(data, JsonConfig);
        byte[] bytes = Encoding.UTF8.GetBytes(dataStr);

        var dataCompressed = Yaz0.Compress(bytes);
        File.WriteAllBytes(Path, dataCompressed.ToArray());

        if (!DebugConfigOutput)
            return true;

        GD.Print(Path.Split(['/', '\\']).Last() + " saved");
        File.WriteAllText(Path + "_d", dataStr);
        return true;
    }

    protected abstract bool TryGetWriteData(out dynamic data);

    #region Utility

    private void AddJsonConverters(bool isStoreColorAlpha)
    {
        JsonConfig.Converters.Add(new GodotColorJsonConverter(isStoreColorAlpha));
        JsonConfig.Converters.Add(new GodotVector2JsonConverter());
    }

    #endregion
}