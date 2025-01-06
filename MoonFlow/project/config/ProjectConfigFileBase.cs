using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using System.Reflection;
using Godot;

using static MoonFlow.Ext.Extension;

using CsYaz0;

namespace MoonFlow.Project;

public abstract class ProjectConfigFileBase
{
    protected string Path = null;
    protected readonly JsonSerializerOptions JsonConfig = new()
    {
        IncludeFields = true,
        IgnoreReadOnlyFields = true,
    };

    // ====================================================== //
    // ==================== Init and Write ================== //
    // ====================================================== //

    public ProjectConfigFileBase(string path, bool isStoreColorAlpha = false)
    {
        JsonConfig.Converters.Add(new GodotColorJsonConverter(isStoreColorAlpha));

        Path = path;
        if (!File.Exists(path))
            return;

        var data = File.ReadAllBytes(path);
        data = Yaz0.Decompress(data);

        var jsonStr = Encoding.UTF8.GetString(data);
        Init(jsonStr);
    }

    public ProjectConfigFileBase(byte[] data, bool isStoreColorAlpha = false)
    {
        JsonConfig.Converters.Add(new GodotColorJsonConverter(isStoreColorAlpha));

        data = Yaz0.Decompress(data);

        var jsonStr = Encoding.UTF8.GetString(data);
        Init(jsonStr);
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

        // Get project config and check if this is a debug project and a debug build
        if (!ProjectManager.IsProjectDebug())
            return true;

        GD.Print(Path.Split(['/', '\\']).Last() + " saved");
        File.WriteAllText(Path + "_d", dataStr);
        return true;
    }

    protected abstract bool TryGetWriteData(out dynamic data);
}

public class GodotColorJsonConverter(bool isStoreAlpha) : JsonConverter<Color>
{
    public bool IsStoreAlpha { get; private set; } = isStoreAlpha;

    public override Color Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        var color = new Color();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;

            var property = Encoding.UTF8.GetString(reader.ValueSpan);

            reader.Read();
            var value = reader.GetInt32();

            switch (property)
            {
                case "R": color.R8 = value; break;
                case "G": color.G8 = value; break;
                case "B": color.B8 = value; break;
                case "A":
                    if (IsStoreAlpha) color.A8 = value;
                    break;
            }
        }

        if (!IsStoreAlpha)
            color.A8 = 255;

        return color;
    }

    public override void Write(Utf8JsonWriter writer, Color color, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber(nameof(Color.R), color.R8);
        writer.WriteNumber(nameof(Color.G), color.G8);
        writer.WriteNumber(nameof(Color.B), color.B8);
        if (IsStoreAlpha)
            writer.WriteNumber(nameof(Color.A), color.A8);

        writer.WriteEndObject();
    }
}