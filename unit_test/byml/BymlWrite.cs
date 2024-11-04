#if TOOLS
using System.IO;
using Godot;

using Nindot.Byml;

namespace Nindot.UnitTest;

public class UnitTestBymlWrite : UnitTestBase
{
    public override void SetupTest()
    {
    }

    public override UnitTestResult Test()
    {
        BymlFile file = BymlFile.FromFilePath("res://unit_test/byml/UnitTest.byml");
        if (file == null)
            return UnitTestResult.FAILURE;
        
        MemoryStream stream = new();
        file.WriteFile(stream);

        var f = Godot.FileAccess.Open("user://unit_test/BymlWrite.byml", Godot.FileAccess.ModeFlags.Write);
        f.StoreBuffer(stream.ToArray());
        f.Close();

        file = BymlFile.FromFilePath("user://unit_test/BymlWrite.byml");
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