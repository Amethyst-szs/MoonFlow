#if TOOLS
using System.Collections.Generic;
using Godot;

using Nindot.Al.EventFlow;
using Nindot.Al.EventFlow.Smo;

namespace Nindot.UnitTest;

public class UnitTestEventDataWrite : IUnitTest
{
    private const string OutFilePath = "user://unit_test/EventFlowGraphOutput.byml";

    public static void SetupTest()
    {
    }

    public static void RunTest()
    {
        Graph flow = Graph.FromFilePath("res://test/byml-eventdata/UnitTest-SphinxQuiz.byml", new ProjectSmoEventFlowFactory());
        Test.Should(flow.IsValid());

        Test.Should(flow.WriteFile(OutFilePath));

        flow = Graph.FromFilePath(OutFilePath, new ProjectSmoEventFlowFactory());
        Test.Should(flow.IsValid());
    }

    public static void CleanupTest()
    {
    }
}

#endif