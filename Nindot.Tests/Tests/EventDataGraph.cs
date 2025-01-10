using System;
using System.IO;
using System.Text;

using BymlLibrary;
using Revrs;

using Nindot.Al.EventFlow;
using Nindot.Al.EventFlow.Smo;

using static Nindot.Tests.PathUtility;

namespace Nindot.Tests;

public class EventDataGraph
{
    [Fact]
    public static void ReadTestGraph()
    {
        Graph flow = Graph.FromFilePath(ResDirectory + "Graph-SphinxQuiz.byml", new ProjectSmoEventFlowFactory());
        Assert.True(flow.IsValid());

        Directory.CreateDirectory(OutputDirectory);
        Assert.True(flow.WriteFile(OutputDirectory + "EventFlowGraphOutput.byml"));

        flow = Graph.FromFilePath(OutputDirectory + "EventFlowGraphOutput.byml", new ProjectSmoEventFlowFactory());
        Assert.True(flow.IsValid());
    }

    [Fact]
    public static void TestAgainstSmo100() { TestWithPath(GetPathSmo100() + "EventData/"); }
    [Fact]
    public static void TestAgainstSmo101() { TestWithPath(GetPathSmo101() + "EventData/"); }
    [Fact]
    public static void TestAgainstSmo110() { TestWithPath(GetPathSmo110() + "EventData/"); }
    [Fact]
    public static void TestAgainstSmo120() { TestWithPath(GetPathSmo120() + "EventData/"); }
    [Fact]
    public static void TestAgainstSmo130() { TestWithPath(GetPathSmo130() + "EventData/"); }
    
    private static void TestWithPath(string path)
    {
        Assert.True(Directory.Exists(path));

        var filePaths = Directory.GetFiles(path, "*.szs");

        foreach (var filePath in filePaths)
        {
            var res = SarcFile.FromFilePath(filePath);
            foreach (var bymlName in res.Content.Keys)
                TestGraph(res, bymlName);
        }
    }
    private static void TestGraph(SarcFile res, string bymlName)
    {
        // Get bytes from sarc
        var bytes = res.Content[bymlName].ToArray();
        Assert.NotEmpty(bytes);

        // Create yaml string of bytes
        RevrsReader bytesReader = new(bytes);
        ImmutableByml bytesByml = new(ref bytesReader);
        string bytesYaml = bytesByml.ToYaml();

        // Create and write graph
        var graph = Graph.FromBytes(bytes, "n/a", new ProjectSmoEventFlowFactory());
        Assert.True(graph.IsValid());
        Assert.True(graph.WriteBytes(out byte[] result));

        // Create yaml string of result
        RevrsReader resReader = new(result);
        ImmutableByml resByml = new(ref resReader);
        string resYaml = resByml.ToYaml();

        // Compare yaml results
        if (bytesYaml != resYaml)
        {
            Directory.CreateDirectory(OutputDirectory);
            File.WriteAllBytes(OutputDirectory + "EventFlowGraphError_Source.txt", Encoding.UTF8.GetBytes(bytesYaml));
            File.WriteAllBytes(OutputDirectory + "EventFlowGraphError_Result.txt", Encoding.UTF8.GetBytes(resYaml));
            File.WriteAllBytes(OutputDirectory + "EventFlowGraphError_Build.byml", result);
            throw new Exception("Mismatch between source and result graph byml!");
        }
    }
}