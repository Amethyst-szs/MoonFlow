#if TOOLS
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BymlLibrary;
using Godot;

using Nindot.Al.EventFlow;
using Nindot.Al.EventFlow.Smo;
using Revrs;

namespace Nindot.UnitTest;

public class UnitTestEventRomFs : IUnitTest
{
    public static void SetupTest()
    {
    }

    public static void RunTest()
    {
        string path = (string)ProjectSettings.GetSetting(UnitTester._130PathKey);
        path += "EventData/";

        Test.Should(DirAccess.DirExistsAbsolute(path));

        var fileList = DirAccess.GetFilesAt(path);
        foreach (var fileName in fileList)
        {
            if (!fileName.EndsWith(".szs"))
                continue;

            var res = SarcResource.FromFilePath(path + fileName);
            foreach (var bymlName in res.GetFileList())
            {
                TestGraph(res, bymlName);
            }
        }
    }

    public static void TestGraph(SarcResource res, string bymlName)
    {
        // Get bytes from sarc
        var bytes = res.GetFile(bymlName);
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
            var src = FileAccess.Open("user://unit_test/EventFlowGraphError_Source.txt", FileAccess.ModeFlags.Write);
            src.StoreString(bytesYaml);
            src.Close();

            var cmp = FileAccess.Open("user://unit_test/EventFlowGraphError_Result.txt", FileAccess.ModeFlags.Write);
            cmp.StoreString(resYaml);
            cmp.Close();

            var resOut = FileAccess.Open("user://unit_test/EventFlowGraphError_Build.byml", FileAccess.ModeFlags.Write);
            resOut.StoreBuffer(result);
            resOut.Close();

            throw new UnitTestException();
        }
    }

    public static void CleanupTest()
    {
    }
}

#endif