using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Nindot.Al.EventFlow;

namespace MoonFlow.Project;

public class GraphMetadataFile : ProjectFileFormatBase<GraphMetaBucketCommon>
{
    private const string PathBase = "EventData/.graph/";
    public const string EmbedGraphPath = "res://scene/editor/event/meta/embed/";

    public GraphMetadataFile(string path) : base("GRPH", path) { }
    public GraphMetadataFile(byte[] data) : base("GRPH", data) { }

    public static GraphMetadataFile Create(SarcEventFlowGraph graph, string projectPath)
    {
        var path = projectPath + PathBase;
        Directory.CreateDirectory(path);

        var fileName = CalcNameHash(graph.Sarc.Name, graph.Name);

        // If file already exists, continue with standard constructor
        if (File.Exists(path + fileName))
            return new GraphMetadataFile(path + fileName);

        // Otherwise, lookup file in the embeded mfgraph directory
        if (Godot.FileAccess.FileExists(EmbedGraphPath + fileName))
        {
            var data = Godot.FileAccess.GetFileAsBytes(EmbedGraphPath + fileName);
            var holder = new GraphMetadataFile(data)
            {
                Path = path + fileName,
            };

            return holder;
        }

        // If all else fails, return a default meta holder
        return new GraphMetadataFile(path + fileName);
    }

    protected override bool TryGetWriteData(out object data)
    {
        data = Data;
        return true;
    }

    public static string CalcNameHash(string archive, string file)
    {
        var input = Encoding.UTF8.GetBytes(archive + file + "SALT_z8anMl1o");

        byte[] hashValue = MD5.HashData(input);
        return BitConverter.ToString(hashValue).Replace("-", string.Empty) + ".mfgraph";
    }

    public static string GetPath(string archive, string file, string projectPath)
    {
        var path = projectPath + PathBase;
        Directory.CreateDirectory(path);

        var fileName = CalcNameHash(archive, file);
        return path + fileName;
    }
}