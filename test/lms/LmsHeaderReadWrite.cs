#if TOOLS

using System.IO;

namespace Nindot.UnitTest;

public class UnitTestLmsHeaderReadWrite : IUnitTest
{
    static private byte[] FileData = [];

    public static void SetupTest()
    {
        FileData = Godot.FileAccess.GetFileAsBytes("res://test/lms/msbt/SmoUnitTesting.msbt");
    }

    public static void RunTest()
    {
        LMS.FileHeader head = new(FileData);
        Test.Should(head.IsValid());

        MemoryStream stream = new();
        Test.Should(head.WriteHeader(stream));
    }

    public static void CleanupTest()
    {
        FileData = null;
    }
}

#endif