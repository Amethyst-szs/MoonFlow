#if TOOLS
using Godot;
using System;

using Nindot.MsbtContent;
using Nindot.MsbtTagLibrary;
using Nindot.MsbtTagLibrary.Smo;

namespace Nindot.UnitTest;

public class UnitTestMsbtSmoWrite : UnitTestMsbtSmoParse
{
    private MsbtResource msbt = null;

    public override void SetupTest()
    {
    }

    public override UnitTestResult Test()
    {
        // Load in an msbt resource and check it's validity
        msbt = MsbtResource.FromFilePath("res://unit_test/msbt/SmoUnitTesting.msbt", Core.Type.SUPER_MARIO_ODYSSEY);
        if (msbt == null || !msbt.IsValid()) {
            GD.PrintErr("Failed to initalize MsbtResource for unit test!");
            return UnitTestResult.FAILURE;
        }

        // Write to disk
        byte[] msbtWrite = MsbtFileAccess.WriteBytes(msbt.Content);
        MsbtFileAccess.WriteDisk("user://UnitTest_MsbtWrite.msbt", msbtWrite);

        // Read in msbt again
        MsbtResource msbtReread = MsbtResource.FromBytes(msbtWrite, Core.Type.SUPER_MARIO_ODYSSEY);
        if (msbt == null || !msbt.IsValid()) {
            GD.PrintErr("Failed to re-read MsbtResource for unit test!");
            return UnitTestResult.FAILURE;
        }

        // Ensure validity of re-read msbt
        return ScanElements(msbtReread, false);
    }

    public override void CleanupTest()
    {
        msbt = null;
    }

    public override void Failure()
    {
    }
}

#endif