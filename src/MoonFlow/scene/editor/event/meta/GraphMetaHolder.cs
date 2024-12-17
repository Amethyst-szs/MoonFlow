using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Nindot.Al.EventFlow;

using MoonFlow.Project;
using System.IO;

namespace MoonFlow.Scene.EditorEvent;

public class GraphMetaHolder(string path) : ProjectConfigFileBase(path)
{
    public GraphMetadata Data { get; private set; } = new();

    private const string PathBase = "EventData/.graph/";

    public static GraphMetaHolder Create(SarcEventFlowGraph graph)
    {
        var path = ProjectManager.GetProject().Path + PathBase;
        Directory.CreateDirectory(path);

        var fileName = CalcNameHash(graph.Sarc.Name, graph.Name);
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

    private static string CalcNameHash(string archive, string file)
    {
        var input = Encoding.UTF8.GetBytes(archive + file + "SALT_z8anMl1o");

        byte[] hashValue = MD5.HashData(input);
        return BitConverter.ToString(hashValue).Replace("-", string.Empty) + ".mfgraph";
    }
}