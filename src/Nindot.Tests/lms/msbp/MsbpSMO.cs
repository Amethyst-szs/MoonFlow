using System.IO;

namespace Nindot.UnitTest;

public class UnitTestMsbpSMO : IUnitTest
{
    static private byte[] FileData = [];

    public static void SetupTest()
    {
        FileData = File.ReadAllBytes("./src/Nindot.Tests/lms/msbp/ProjectData-SMO.msbp");
    }

    public static void RunTest()
    {
        // Load in file
        LMS.Msbp.MsbpFile file = new(FileData);
        Test.Should(file.IsValid());

        // Test color reading
        Test.Should(file.Color_IsFileContainData());
        Test.Should(file.Color_GetLabel(0), "Black");

        MemoryStream stream = new();
        Test.Should(file.WriteFile(stream));

        File.WriteAllBytes(Test.TestOutputDirectory + "MsbpSMO.msbp", stream.ToArray());
    }

    public static void CleanupTest()
    {
        FileData = null;
    }
}