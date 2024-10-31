#if TOOLS
using Godot;
using System;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;
using Nindot.LMS.Msbp;

namespace Nindot.UnitTest;

public class UnitTestMsbtV2SMOParse : UnitTestBase
{
    protected byte[] FileData = [];

    public override void SetupTest()
    {
        FileData = FileAccess.GetFileAsBytes("res://unit_test/msbt/SmoUnitTesting_TSY.msbt");
    }

    public override UnitTestResult Test()
    {
        // Load in an msbt resource and check it's validity
        MsbtFile msbt = new(FileData);
        if (msbt == null || !msbt.IsValid()) {
            GD.PrintErr("Failed to initalize MsbtV2 for unit test!");
            return UnitTestResult.FAILURE;
        }

        return UnitTestResult.OK;
        // return ScanElements(msbt);
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