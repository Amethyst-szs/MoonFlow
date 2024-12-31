using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Nindot.Al.EventFlow;

using MoonFlow.Project;
using System.IO;

namespace MoonFlow.Scene.EditorEvent;

public class GraphMetaHolder : ProjectConfigFileBase
{
    public GraphMetadata Data { get; private set; } = new();
    private const string PathBase = "EventData/.graph/";
    private const string EmbedGraphPath = "res://project/event/embed/";

    public GraphMetaHolder(string path) : base(path) { }
    public GraphMetaHolder(byte[] data) : base(data) { }

    public static GraphMetaHolder Create(SarcEventFlowGraph graph)
    {
        var path = ProjectManager.GetProject().Path + PathBase;
        Directory.CreateDirectory(path);

        var fileName = CalcNameHash(graph.Sarc.Name, graph.Name);

        // If file already exists, continue with standard constructor
        if (File.Exists(path + fileName))
            return new GraphMetaHolder(path + fileName);

        // Otherwise, lookup file in the embeded mfgraph directory
        if (Godot.FileAccess.FileExists(EmbedGraphPath + fileName))
        {
            var data = Godot.FileAccess.GetFileAsBytes(EmbedGraphPath + fileName);
            var holder = new GraphMetaHolder(data)
            {
                Path = path + fileName,
            };

            return holder;
        }

        // If all else fails, return a default meta holder
        return new GraphMetaHolder(path + fileName);
    }

    protected override void Init(string json)
    {
        Data = JsonSerializer.Deserialize<GraphMetadata>(json, JsonConfig);
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
    
    public static string GetPath(string archive, string file)
    {
        var path = ProjectManager.GetProject().Path + PathBase;
        Directory.CreateDirectory(path);

        var fileName = CalcNameHash(archive, file);
        return path + fileName;
    }
}