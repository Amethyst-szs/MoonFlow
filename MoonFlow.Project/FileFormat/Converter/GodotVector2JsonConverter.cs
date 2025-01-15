using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace MoonFlow.Project;

public class GodotVector2JsonConverter() : JsonConverter<Vector2>
{
    public override Vector2 Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        var vec = new Vector2();

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
                case "X": vec.X = value; break;
                case "Y": vec.Y = value; break;
            }
        }

        return vec;
    }

    public override void Write(Utf8JsonWriter writer, Vector2 vec, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber(nameof(Vector2.X), vec.X);
        writer.WriteNumber(nameof(Vector2.Y), vec.Y);
        writer.WriteEndObject();
    }
}