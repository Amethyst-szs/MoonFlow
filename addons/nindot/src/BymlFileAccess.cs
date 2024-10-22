using Godot;
using System;
using System.Buffers;
using System.Collections.Generic;

using BymlLibrary;
using Revrs;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public partial class BymlFileAccess : Node
{
    public static Dictionary<object, object> ParseBytes(byte[] data)
    {
        // Create an imutable byml from bytes
        RevrsReader reader = new(data);
        ImmutableByml byml = new(ref reader);

        // Convert this byml to yaml string
        string yamlString = byml.ToYaml();

        // Convert yaml string to C# dictionary
        IDeserializer deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        object yaml = deserializer.Deserialize(yamlString);

        // Ensure the yaml deserialize was successful
        if (yaml.GetType() != typeof(Dictionary<object, object>))
            return [];

        return (Dictionary<object, object>)yaml;
    }

    public static Dictionary<object, object> ParsePath(string path)
    {
        if (!FileAccess.FileExists(path))
            return [];

        return ParseBytes(FileAccess.GetFileAsBytes(path));
    }

    public static bool WriteMemoryStream(string path, Dictionary<object, object> dict, out System.IO.MemoryStream stream)
    {
        // Setup out
        stream = new System.IO.MemoryStream();

        // Ensure path is valid
        if (!DirAccess.DirExistsAbsolute(path.GetBaseDir()))
            return false;
        
        // Convert dictionary to yaml string
        ISerializer serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        
        string yaml = serializer.Serialize(dict);

        // Use string to create byml
        Byml byml = Byml.FromText(yaml);

        // Write this byml to the out stream
        byml.WriteBinary(stream, Endianness.Little);

        return true;
    }

    public static bool WriteDisk(string path, Dictionary<object, object> dict)
    {
        System.IO.MemoryStream stream;
        if (!WriteMemoryStream(path, dict, out stream))
            return false;

        // Write memory stream to disk using Godot
        FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file.StoreBuffer(stream.GetBuffer());
        file.Close();

        return true;
    }
}