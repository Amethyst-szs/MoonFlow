#if TOOLS

using System.IO;
using Godot;

using Nindot.Byml;

namespace Nindot.UnitTest;

public class UnitTestBymlAccess : IUnitTest
{
    public static void SetupTest()
    {
    }

    public static void RunTest()
    {
        BymlFile file = BymlFile.FromFilePath("res://test/byml/UnitTest.byml");
        Test.ShouldNot(file, null);

        MemoryStream stream = new();
        Test.Should(file.WriteFile(stream));

        var f = Godot.FileAccess.Open("user://unit_test/BymlWrite.byml", Godot.FileAccess.ModeFlags.Write);
        Test.Should(Godot.FileAccess.GetOpenError() == Error.Ok);

        f.StoreBuffer(stream.ToArray());
        f.Close();

        file = BymlFile.FromFilePath("user://unit_test/BymlWrite.byml");
        Test.ShouldNot(file, null);
    }

    public static void CleanupTest()
    {
    }
}

#endif