#if TOOLS
using System.Collections.Generic;
using Godot;

using Nindot.Al.EventFlow;
using Nindot.Al.EventFlow.Smo;

namespace Nindot.UnitTest;

public class UnitTestEventDataRead : UnitTestBase
{
    public override void SetupTest()
    {
    }

    public override UnitTestResult Test()
    {
        Graph flow = Graph.FromFilePath("res://unit_test/byml-eventdata/UnitTest.byml", new ProjectSmoEventFlowFactory());
        if (!flow.IsValid())
            return UnitTestResult.FAILURE;

        return UnitTestResult.OK;
    }

    public override void CleanupTest()
    {
    }

    public override void Failure()
    {
    }
}

#endif