using System;
using System.IO;
using System.Collections.Generic;

using BymlLibrary;
using Revrs;

using YamlDotNet.Serialization;

namespace Nindot.Byml;

public class BymlFileAccess
{
    #region Parse

    public static bool ParseBytes(out BymlFile iter, byte[] data)
    {
        var yamlStr = GetYamlString(data, out ushort verison);

        IDeserializer deserializer = GetDeserializer();
        var yaml = deserializer.Deserialize<Dictionary<string, object>>(yamlStr);

        iter = new BymlFile(yaml, verison);
        return true;
    }

    public static T ParseBytes<T>(byte[] data)
    {
        var yamlStr = GetYamlString(data, out ushort verison);

        IDeserializer deserializer = GetDeserializer();
        var yaml = deserializer.Deserialize<T>(yamlStr);

        return yaml;
    }

    private static IDeserializer GetDeserializer()
    {
        return new DeserializerBuilder()
            .WithTagMapping("!s", typeof(string))
            .WithTagMapping("!b", typeof(bool))
            .WithTagMapping("!l", typeof(int))
            .WithTagMapping("!ll", typeof(long))
            .WithTagMapping("!u", typeof(uint))
            .WithTagMapping("!ul", typeof(ulong))
            .WithTagMapping("!f", typeof(float))
            .WithTagMapping("!d", typeof(double))
            .IgnoreUnmatchedProperties()
            .Build();
    }

    private static string GetYamlString(byte[] data, out ushort version)
    {
        // Create an immutable byml from bytes
        RevrsReader reader = new(data);
        ImmutableByml byml = new(ref reader);

        version = byml.Header.Version;

        // Check to see if the byml file is completely empty
        if (byml.Header.StringTableOffset == 0 && byml.Header.RootNodeOffset == 0)
            return "";

        // Convert this byml to yaml string
        string yamlString = byml.ToYaml();
        return yamlString;
    }

    public static bool ParseFile(out BymlFile iter, string path)
    {
        iter = null;
        if (!File.Exists(path))
            return false;

        return ParseBytes(out iter, File.ReadAllBytes(path));
    }

    #endregion

    #region Writing

    public static bool WriteFile(MemoryStream stream, BymlFile iter, IYamlTypeConverter typeConverter, ushort version = 3)
    {
        return WriteFile<Dictionary<string, object>>(stream, iter, typeConverter, version);
    }

    public static bool WriteFile<T>(MemoryStream stream, T input, IYamlTypeConverter typeConverter, ushort version = 3)
    {
        // Convert dictionary to yaml string
        typeConverter ??= new NindotCoreYamlTypeConverter();

        ISerializer serializer = new SerializerBuilder()
            .WithTypeConverter(typeConverter)
            .Build();

        string yaml = serializer.Serialize(input, typeof(T));

        // Convert scalar anchor placeholders into tags
        yaml = yaml.Replace("&⌂♯", "");

        // Use string to create byml
        BymlLibrary.Byml byml = BymlLibrary.Byml.FromText(yaml);

        // Write this byml to the out stream
        byml.WriteBinary(stream, Endianness.Little, version);

        return true;
    }

    #endregion
}