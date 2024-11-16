#if TOOLS
using Godot;

namespace Nindot.UnitTest;

public class UnitTestMsbp3DW : IUnitTest
{
    static private byte[] FileData = [];

    public static void SetupTest()
    {
        FileData = FileAccess.GetFileAsBytes("res://test/lms/msbp/ProjectData-3DW-BF.msbp");
    }

    public static void RunTest()
    {
        // Load in file
        LMS.Msbp.MsbpFile file = new(FileData);
        Test.Should(file.IsValid());

        // Test color reading
        Test.Should(file.Color_IsFileContainData());
        Test.Should(file.Color_GetLabel(0), "White");

        System.IO.MemoryStream stream = new();
        Test.Should(file.WriteFile(stream));

        FileAccess writer = FileAccess.Open("user://unit_test/Msbp3DW.msbp", FileAccess.ModeFlags.Write);
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