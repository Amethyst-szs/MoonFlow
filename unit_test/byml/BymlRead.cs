#if TOOLS
using Godot;

using Nindot.Byml;

namespace Nindot.UnitTest;

public class UnitTestBymlRead : UnitTestBase
{
    public override void SetupTest()
    {
    }

    public override UnitTestResult Test()
    {
        BymlFile file = BymlFile.FromFilePath("res://unit_test/byml/UnitTest.byml");
        if (file == null)
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