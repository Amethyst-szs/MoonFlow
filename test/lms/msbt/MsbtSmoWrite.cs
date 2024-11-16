#if TOOLS
using Godot;
using System;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;
using System.Util;

namespace Nindot.UnitTest;

public class UnitTestMsbtSMOWrite : IUnitTest
{
    private static byte[] FileData = [];
    private const string FileOutPath = "user://unit_test/MsbtSMO.msbt";

    public static void SetupTest()
    {
        FileData = FileAccess.GetFileAsBytes("res://test/lms/msbt/SmoUnitTesting.msbt");
    }

    public static void RunTest()
    {
        // Load in an msbt resource and check it's validity
        MsbtFile msbt = new(new MsbtElementFactoryProjectSmo(), FileData);
        Test.Should(msbt.IsValid());

        UnitTestMsbtSMOParse.TestAllElements(msbt.Content);

        // Write file to stream
        System.IO.MemoryStream stream = new();
        Test.Should(msbt.WriteFile(stream));

        // Write msbt to disk
        FileAccess writer = FileAccess.Open(FileOutPath, FileAccess.ModeFlags.Write);
        Test.Should(FileAccess.GetOpenError() == Error.Ok);

        writer.StoreBuffer(stream.ToArray());
        writer.Close();

        // Read the msbt back in from write
        FileData = FileAccess.GetFileAsBytes(FileOutPath);
        msbt = new(new MsbtElementFactoryProjectSmo(), FileData);
        Test.Should(msbt.IsValid());
    }

    public static void CleanupTest()
    {
        FileData = null;
    }
}

#endif