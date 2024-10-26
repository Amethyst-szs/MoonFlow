using Godot;
using System.Collections.Generic;

using BymlLibrary;
using Revrs;

using YamlDotNet.Serialization;

namespace Nindot
{
    public partial class BymlFileAccess : GodotObject
    {
        public static bool ParseBytes(out BymlIter iter, byte[] data)
        {
            // Create an imutable byml from bytes
            RevrsReader reader = new(data);
            ImmutableByml byml = new(ref reader);

            // Convert this byml to yaml string
            string yamlString = byml.ToYaml();

            // Convert yaml string to C# dictionary
            IDeserializer deserializer = new DeserializerBuilder()
                .WithTagMapping("!s", typeof(string))
                .WithTagMapping("!b", typeof(bool))
                .WithTagMapping("!l", typeof(int))
                .WithTagMapping("!ll", typeof(long))
                .WithTagMapping("!u", typeof(uint))
                .WithTagMapping("!ul", typeof(ulong))
                .WithTagMapping("!f", typeof(float))
                .WithTagMapping("!d", typeof(double))
                .Build();

            Dictionary<string, object> yaml = deserializer.Deserialize<Dictionary<string, object>>(yamlString);
            iter = new BymlIter(yaml);

            return true;
        }

        public static bool ParseFile(out BymlIter iter, string path)
        {
            iter = null;
            if (!FileAccess.FileExists(path))
                return false;

            return ParseBytes(out iter, FileAccess.GetFileAsBytes(path));
        }

        public static bool WriteStream(BymlIter iter, out System.IO.MemoryStream stream)
        {
            // Setup out
            stream = new System.IO.MemoryStream();

            // Convert dictionary to yaml string
            ISerializer serializer = new SerializerBuilder()
                .WithTagMapping("!s", typeof(string))
                .WithTagMapping("!b", typeof(bool))
                .WithTagMapping("!l", typeof(int))
                .WithTagMapping("!ll", typeof(long))
                .WithTagMapping("!u", typeof(uint))
                .WithTagMapping("!ul", typeof(ulong))
                .WithTagMapping("!f", typeof(float))
                .WithTagMapping("!d", typeof(double))
                .Build();

            string yaml = serializer.Serialize(iter);

            // Use string to create byml
            Byml byml = Byml.FromText(yaml);

            // Write this byml to the out stream
            byml.WriteBinary(stream, Endianness.Little);

            return true;
        }

        public static bool WriteDisk(string path, BymlIter iter)
        {
            // Ensure path is valid
            if (!DirAccess.DirExistsAbsolute(path.GetBaseDir()))
                return false;

            // Create memory stream to later write to disk
            System.IO.MemoryStream stream;
            if (!WriteStream(iter, out stream))
                return false;

            // Write memory stream to disk using Godot
            FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
            file.StoreBuffer(stream.GetBuffer());
            file.Close();

            return true;
        }
    }
}