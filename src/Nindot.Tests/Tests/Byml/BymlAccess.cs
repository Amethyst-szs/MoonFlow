using System.IO;

using Nindot.Byml;

namespace Nindot.UnitTest;

public class UnitTestBymlAccess : IUnitTestGroup
{
    public static void SetupGroup()
    {
    }

    [RunTest]
    public static void ReadByml()
    {
        BymlFile file = BymlFile.FromFilePath("./src/Nindot.Tests/Resources/UnitTest.byml");
        Test.Should(file != null);
    }

    [RunTest]
    public static void WriteByml()
    {
        BymlFile file = BymlFile.FromFilePath("./src/Nindot.Tests/Resources/UnitTest.byml");
        Test.Should(file != null);

        MemoryStream stream = new();
        Test.Should(file.WriteFile(stream));

        File.WriteAllBytes(Test.TestOutputDirectory + "BymlWrite.byml", stream.ToArray());

        file = BymlFile.FromFilePath(Test.TestOutputDirectory + "BymlWrite.byml");
        Test.Should(file != null);
    }

    public static void CleanupGroup()
    {
    }
}