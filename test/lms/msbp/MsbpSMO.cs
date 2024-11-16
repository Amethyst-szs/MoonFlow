#if TOOLS
using Godot;

namespace Nindot.UnitTest;

public class UnitTestMsbpSMO : IUnitTest
{
    static private byte[] FileData = [];

    public static void SetupTest()
    {
        FileData = FileAccess.GetFileAsBytes("res://test/lms/msbp/ProjectData-SMO.msbp");
    }

    public static void RunTest()
    {
        // Load in file
        LMS.Msbp.MsbpFile file = new(FileData);
        Test.Should(file.IsValid());

        // Test color reading
        Test.Should(file.Color_IsFileContainData());
        Test.Should(file.Color_GetLabel(0), "Black");

        System.IO.MemoryStream stream = new();
        Test.Should(file.WriteFile(stream));

        FileAccess writer = FileAccess.Open("user://unit_test/MsbpSMO.msbp", FileAccess.ModeFlags.Write);
        Test.Should(FileAccess.GetOpenError(), Error.Ok);

        writer.StoreBuffer(stream.ToArray());
        writer.Close();
    }

    public static void CleanupTest()
    {
        FileData = null;
    }
}

#endif