using System.IO;
using System.Text;
using BymlLibrary;

using Nindot.Al.EventFlow;
using Nindot.Al.EventFlow.Smo;
using Revrs;

namespace Nindot.UnitTest;

public class UnitTestEventRomFs : IUnitTestGroup
{
    public static void SetupGroup()
    {
    }

    [RunTest, SmoRomfsTest]
    public static void TestEventDataRomfs()
    {
        var path = Test.RomfsDirectory + "EventData/";
        Test.Should(Directory.Exists(path));

        var filePaths = Directory.GetFiles(path, "*.szs");

        foreach (var filePath in filePaths)
        {
            var res = SarcFile.FromFilePath(filePath);
            foreach (var bymlName in res.Content.Keys)
            {
                TestGraph(res, bymlName);
            }
        }
    }

    public static void TestGraph(SarcFile res, string bymlName)
    {
        // Get bytes from sarc
        var bytes = res.Content[bymlName].ToArray();
        Test.ShouldNot(bytes.Length == 0);

        // Create yaml string of bytes
        RevrsReader bytesReader = new(bytes);
        ImmutableByml bytesByml = new(ref bytesReader);
        string bytesYaml = bytesByml.ToYaml();

        // Create and write graph
        var graph = Graph.FromBytes(bytes, new ProjectSmoEventFlowFactory());
        Test.Should(graph.IsValid());
        Test.Should(graph.WriteBytes(out byte[] result));

        // Create yaml string of result
        RevrsReader resReader = new(result);
        ImmutableByml resByml = new(ref resReader);
        string resYaml = resByml.ToYaml();

        // Compare yaml results
        if (bytesYaml != resYaml)
        {
            File.WriteAllBytes(Test.TestOutputDirectory + "EventFlowGraphError_Source.txt", Encoding.UTF8.GetBytes(bytesYaml));
            File.WriteAllBytes(Test.TestOutputDirectory + "EventFlowGraphError_Result.txt", Encoding.UTF8.GetBytes(resYaml));
            File.WriteAllBytes(Test.TestOutputDirectory + "EventFlowGraphError_Build.byml", result);
            throw new UnitTestException();
        }
    }

    public static void CleanupGroup()
    {
    }
}