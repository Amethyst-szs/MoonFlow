using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

using CsYaz0;

namespace MoonFlow.Project;

public abstract class ProjectConfigFileBase
{
    protected readonly string Path = null;
    protected readonly JsonSerializerOptions JsonConfig = new()
    {
        IncludeFields = true,
        IgnoreReadOnlyFields = true,
    };

    // ====================================================== //
    // ==================== Init and Write ================== //
    // ====================================================== //

    public ProjectConfigFileBase(string path)
    {
        JsonConfig.Converters.Add(new GodotColorJsonConverter());

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

        if (OS.IsDebugBuild())
            File.WriteAllText(Path + "_d", dataStr);

        return true;
    }

    protected abstract bool TryGetWriteData(out object data);
}

public class GodotColorJsonConverter : JsonConverter<Color>
{
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
                case "A": color.A8 = value; break;
            }
        }

        return color;
    }

    public override void Write(Utf8JsonWriter writer, Color color, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber(nameof(Color.R), color.R8);
        writer.WriteNumber(nameof(Color.G), color.G8);
        writer.WriteNumber(nameof(Color.B), color.B8);
        writer.WriteNumber(nameof(Color.A), color.A8);
        writer.WriteEndObject();
    }
}