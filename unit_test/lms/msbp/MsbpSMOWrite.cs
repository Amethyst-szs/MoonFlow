#if TOOLS
using Godot;

namespace Nindot.UnitTest;

public class UnitTestMsbpSMOWrite : UnitTestBase
{
    protected byte[] FileData = [];

    public override void SetupTest()
    {
        FileData = FileAccess.GetFileAsBytes("res://unit_test/lms/msbp/ProjectData-SMO.msbp");
    }

    public override UnitTestResult Test()
    {
        LMS.Msbp.MsbpFile file = new(FileData);
        if (!file.IsValid())
            return UnitTestResult.FAILURE;

        System.IO.MemoryStream stream = new();
        if (!file.WriteFile(stream))
            return UnitTestResult.FAILURE;

        FileAccess writer = FileAccess.Open("user://unit_test/MsbpSMO.msbp", FileAccess.ModeFlags.Write);
        writer.StoreBuffer(stream.ToArray());
        writer.Close();

        return UnitTestResult.OK;
    }

    public override void CleanupTest()
    {
        FileData = null;
    }

    public override void Failure()
    {
    }
}

#endif