using System.IO;

using Nindot.Byml;

namespace Nindot.UnitTest;

public class UnitTestBymlAccess : IUnitTest
{
    public static void SetupTest()
    {
    }

    public static void RunTest()
    {
        BymlFile file = BymlFile.FromFilePath("./src/Nindot.Tests/Tests/Byml/UnitTest.byml");
        Test.Should(file != null);

        MemoryStream stream = new();
        Test.Should(file.WriteFile(stream));

        File.WriteAllBytes(Test.TestOutputDirectory + "BymlWrite.byml", stream.ToArray());

        file = BymlFile.FromFilePath(Test.TestOutputDirectory + "BymlWrite.byml");
        Test.Should(file != null);
    }

    public static void CleanupTest()
    {
    }
}