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

public class UnitTestEventDataSourceValidity : UnitTestBase
{
    public override void SetupTest()
    {
    }

    public override UnitTestResult Test()
    {
        string path = (string)ProjectSettings.GetSetting(UnitTester._130PathKey);
        path += "EventData/";
        if (!DirAccess.DirExistsAbsolute(path))
            return UnitTestResult.FAILURE;
        
        var fileList = DirAccess.GetFilesAt(path);
        foreach (var fileName in fileList)
        {
            if (!fileName.EndsWith(".szs"))
                continue;
            
            var res = SarcResource.FromFilePath(path + fileName);
            foreach (var bymlName in res.GetFileList())
            {
                if (!TestGraph(res, bymlName))
                    return UnitTestResult.FAILURE;
            }
        }

        return UnitTestResult.OK;
    }

    public bool TestGraph(SarcResource res, string bymlName)
    {
        var bytes = res.GetFile(bymlName);
        if (bytes.Length == 0)
            return false;
        
        // Create yaml string of bytes
        RevrsReader bytesReader = new(bytes);
        ImmutableByml bytesByml = new(ref bytesReader);
        string bytesYaml = bytesByml.ToYaml();
        
        var graph = Graph.FromBytes(bytes, new ProjectSmoEventFlowFactory());
        if (graph == null || !graph.IsValid())
            return false;
        
        if (!graph.WriteBytes(out byte[] result))
            return false;
        
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

            return false;
        }

        return true;
    }

    public override void CleanupTest()
    {
    }

    public override void Failure()
    {
    }
}

#endif