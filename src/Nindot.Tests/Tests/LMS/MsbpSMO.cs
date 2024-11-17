using System.IO;
using Nindot.LMS.Msbp;

namespace Nindot.UnitTest;

public class UnitTestMsbpProjectSMO : IUnitTestGroup
{
    public static void SetupGroup()
    {
    }

    [RunTest, SmoRomfsTest]
    public static void MsbpParse()
    {
        // Get access to ProjectData sarc
        SarcFile sarc = SarcFile.FromFilePath(Test.RomfsDirectory + "LocalizedData/Common/ProjectData.szs");

        // Load in file
        MsbpFile file = sarc.GetFileMSBP("ProjectData.msbp");
        Test.Should(file.IsValid());

        // Test individiual blocks
        Test.Should(file.Color_IsFileContainData());
        Test.Should(file.Color_GetCount(), 8);
        Test.Should(file.Color_GetLabel(0), "Black");
        Test.Should(file.Color_Get(0), new BlockColor.Entry(0, 0, 0, 255));
        Test.Should(file.Color_GetLabel(1), "Yellow");
        Test.Should(file.Color_Get(1), new BlockColor.Entry(255, 255, 0, 255));
    }

    [RunTest, SmoRomfsTest]
    public static void MsbpWrite()
    {
        // Get access to ProjectData sarc
        SarcFile sarc = SarcFile.FromFilePath(Test.RomfsDirectory + "LocalizedData/Common/ProjectData.szs");

        // Load in file
        MsbpFile file = sarc.GetFileMSBP("ProjectData.msbp");
        Test.Should(file.IsValid());

        MemoryStream stream = new();
        Test.Should(file.WriteFile(stream));

        File.WriteAllBytes(Test.TestOutputDirectory + "MsbpSMO.msbp", stream.ToArray());
    }

    public static void CleanupGroup()
    {
    }
}