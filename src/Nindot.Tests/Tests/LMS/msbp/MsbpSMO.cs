using System.IO;

namespace Nindot.UnitTest;

public class UnitTestMsbpSMO : IUnitTestGroup
{
    static private byte[] FileData = [];

    public static void SetupGroup()
    {
        FileData = File.ReadAllBytes("./src/Nindot.Tests/Tests/LMS/msbp/ProjectData-SMO.msbp");
    }

    [RunTest]
    public static void MsbpReadWrite()
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

    public static void CleanupGroup()
    {
        FileData = null;
    }
}