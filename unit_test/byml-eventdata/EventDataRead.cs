#if TOOLS
using System.Collections.Generic;
using Godot;

using Nindot.Al.EventFlow;

namespace Nindot.UnitTest;

public class UnitTestEventDataRead : UnitTestBase
{
    public override void SetupTest()
    {
    }

    public override UnitTestResult Test()
    {
        Graph flow = Graph.FromFilePath("res://unit_test/byml-eventdata/UnitTest.byml", GraphGameType.SUPER_MARIO_ODYSSEY);
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