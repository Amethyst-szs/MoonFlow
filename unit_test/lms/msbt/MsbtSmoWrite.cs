#if TOOLS
using Godot;
using System;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;
using Nindot.LMS.Msbp;

namespace Nindot.UnitTest;

public class UnitTestMsbtSMOWrite : UnitTestMsbtSMOParse
{
    public override void SetupTest()
    {
        FileData = FileAccess.GetFileAsBytes("res://unit_test/lms/msbt/SmoUnitTesting.msbt");
    }

    public override UnitTestResult Test()
    {
        // Load in an msbt resource and check it's validity
        MsbtFile msbt = new(new MsbtElementFactoryProjectSmo(), FileData);
        if (!msbt.IsValid())
        {
            GD.PrintErr("Failed to initalize MsbtV2 for unit test!");
            return UnitTestResult.FAILURE;
        }

        // Write this msbt to a file
        System.IO.MemoryStream stream = new();
        if (!msbt.WriteFile(stream))
        {
            GD.PrintErr("Failed to write MsbtV2 for unit test!");
            return UnitTestResult.FAILURE;
        }

        FileAccess writer = FileAccess.Open("user://unit_test/MsbtSMO.msbt", FileAccess.ModeFlags.Write);
        writer.StoreBuffer(stream.ToArray());
        writer.Close();

        // Read this output back in and run element testing on it
        msbt = new(new MsbtElementFactoryProjectSmo(), FileAccess.GetFileAsBytes("user://unit_test/MsbtSMO.msbt"));
        if (!msbt.IsValid())
        {
            GD.PrintErr("Failed to initalize MsbtV2 for unit test!");
            return UnitTestResult.FAILURE;
        }

        return ScanElements(msbt.Content);
    }

    public override void CleanupTest()
    {
        FileData = null;
    }

    public override void Failure()
    {
    }
}

#endif