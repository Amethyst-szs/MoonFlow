using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;
using Godot;

using System;

using AuroraLib.Compression.Algorithms;
using Nindot;

namespace MoonFlow.Project;

public abstract class ProjectFileFormatBase<T> where T : IProjectFileFormatDataRoot, new()
{
    protected string Path = null;
    public bool IsReadFromDisk { get; private set; } = false;

    public T Data = new();

    private string __magic = null;
    private string MagicSignature
    {
        get
        {
            if (__magic == null)
                throw new NullReferenceException("File magic not defined!");

            return __magic;
        }
        set
        {
            if (value.Length != MagicLength)
                throw new Exception("Invalid magic signature, must be MagicLength characters");

            __magic = value;
        }
    }

    private const int MagicLength = 4;

    protected readonly JsonSerializerOptions JsonConfig = new();

    #region Initilization

    public ProjectFileFormatBase(string magic, bool isStoreColorAlpha = false)
    {
        MagicSignature = magic;
        AddJsonConverters(isStoreColorAlpha);

        IsReadFromDisk = true;
    }

    public ProjectFileFormatBase(string magic, string path, bool isStoreColorAlpha = false)
    {
        MagicSignature = magic;
        AddJsonConverters(isStoreColorAlpha);

        Init(path);
    }

    public ProjectFileFormatBase(string magic, byte[] data, bool isStoreColorAlpha = false)
    {
        MagicSignature = magic;
        AddJsonConverters(isStoreColorAlpha);

        Init(data);
    }

    private void Init(string path)
    {
        Path = path;
        if (!File.Exists(path))
            return;

        var buffer = File.ReadAllBytes(path);
        Init(buffer);
    }

    private void Init(byte[] buffer)
    {
        // Compare first four bytes to magic signature
        if (Encoding.UTF8.GetString(buffer.AsSpan()[..MagicLength]) != MagicSignature)
            throw new Exception("Invalid file magic signature!");

        buffer = NindotYaz0.Decompress(buffer[MagicLength..]);

        Data = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(buffer), JsonConfig);
        IsReadFromDisk = true;
    }

    #endregion

    #region Writing

    public void ChangeWritePath(string path) { Path = path; }
    protected abstract bool TryGetWriteData(out dynamic data);

    public bool WriteFile()
    {
        if (!TryGetWriteData(out object data))
            return false;

        // Convert data class into json bytes
        string dataStr = JsonSerializer.Serialize(data, JsonConfig);
        byte[] bytes = Encoding.UTF8.GetBytes(dataStr);

        // Create bytecode version of magic signature and compress data
        var sig = Encoding.UTF8.GetBytes(MagicSignature);
        var compressionResult = NindotYaz0.Compress(bytes);
        
        // Write signature and compressed data to file
        var output = sig.Concat(compressionResult).ToArray();
        File.WriteAllBytes(Path, output);

        if (!DebugConfigOutput)
            return true;

        GD.Print(Path.Split(['/', '\\']).Last() + " saved");
        File.WriteAllText(Path + "_d", dataStr);
        return true;
    }

    #endregion

    #region Utility

    private void AddJsonConverters(bool isStoreColorAlpha)
    {
        JsonConfig.Converters.Add(new GodotColorJsonConverter(isStoreColorAlpha));
        JsonConfig.Converters.Add(new GodotVector2JsonConverter());
    }

    #endregion
}