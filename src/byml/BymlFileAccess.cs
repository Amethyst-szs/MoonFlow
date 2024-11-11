using System;
using Godot;
using System.Collections.Generic;

using BymlLibrary;
using Revrs;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeResolvers;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Nindot.Byml;

public class BymlFileAccess
{
    public static bool ParseBytes(out BymlFile iter, byte[] data)
    {
        // Create an imutable byml from bytes
        RevrsReader reader = new(data);
        ImmutableByml byml = new(ref reader);

        // Convert this byml to yaml string
        string yamlString = byml.ToYaml();

        // Convert yaml string to dictionary
        IDeserializer deserializer = new DeserializerBuilder()
            .WithTagMapping("!s", typeof(string))
            .WithTagMapping("!b", typeof(bool))
            .WithTagMapping("!l", typeof(int))
            .WithTagMapping("!ll", typeof(long))
            .WithTagMapping("!u", typeof(uint))
            .WithTagMapping("!ul", typeof(ulong))
            .WithTagMapping("!f", typeof(float))
            .WithTagMapping("!d", typeof(double))
            .WithTypeResolver(new StaticTypeResolver())
            .Build();

        Dictionary<string, object> yaml = deserializer.Deserialize<Dictionary<string, object>>(yamlString);
        iter = new BymlFile(yaml, byml.Header.Version);

        return true;
    }

    public static bool ParseFile(out BymlFile iter, string path)
    {
        iter = null;
        if (!FileAccess.FileExists(path))
            return false;

        return ParseBytes(out iter, FileAccess.GetFileAsBytes(path));
    }

    public static bool WriteFile(System.IO.MemoryStream stream, BymlFile iter, ushort version = 3)
    {
        // Convert dictionary to yaml string
        ISerializer serializer = new SerializerBuilder()
            .WithTypeConverter(new YamlTypeTagMapper())
            .EnsureRoundtrip()
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
            emitter.Emit(new Scalar("⌂♯" + Table[type], null, value.ToString()));
        }
    }
}