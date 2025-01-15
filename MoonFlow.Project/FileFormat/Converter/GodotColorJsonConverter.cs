using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace MoonFlow.Project;

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