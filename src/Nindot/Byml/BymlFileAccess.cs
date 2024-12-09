using System;
using System.IO;
using System.Collections.Generic;

using BymlLibrary;
using Revrs;

using YamlDotNet.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

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

    public static bool WriteFile(MemoryStream stream, BymlFile iter, ushort version = 3)
    {
        // Convert dictionary to yaml string
        ISerializer serializer = new SerializerBuilder()
            .WithTypeConverter(new YamlTypeTagMapper())
            .Build();

        string yaml = serializer.Serialize(iter, typeof(Dictionary<string, object>));

        // Convert scalar anchor placeholders into tags
        yaml = yaml.Replace("&⌂♯", "");

        // Use string to create byml
        BymlLibrary.Byml byml = BymlLibrary.Byml.FromText(yaml);

        // Write this byml to the out stream
        byml.WriteBinary(stream, Endianness.Little, version);

        return true;
    }

    public class YamlTypeTagMapper : IYamlTypeConverter
    {
        public static readonly Dictionary<Type, string> Table = new(){
            { typeof(bool), "!b" },
            { typeof(string), "!s" },
            { typeof(int), "!l" },
            { typeof(long), "!ll" },
            { typeof(uint), "!u" },
            { typeof(ulong), "!ul" },
            { typeof(float), "!f" },
            { typeof(double), "!d" },
        };

        public bool Accepts(Type type)
        {
            return Table.ContainsKey(type);
        }
        public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            throw new NotImplementedException();
        }
        public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
        {
            emitter.Emit(new Scalar("⌂♯" + Table[type], null, value.ToString(), ScalarStyle.Any, true, false));
        }
    }

    #endregion
}