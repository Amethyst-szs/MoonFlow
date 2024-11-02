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
    protected byte[] MsbpData = [];

    public override void SetupTest()
    {
        FileData = FileAccess.GetFileAsBytes("res://unit_test/msbt/SmoUnitTesting.msbt");
        MsbpData = FileAccess.GetFileAsBytes("res://unit_test/msbp/ProjectData-SMO.msbp");
    }

    public override UnitTestResult Test()
    {
        // Load in an msbt resource and check it's validity
        MsbtFile msbt = new(TagLibraryHolder.Type.SUPER_MARIO_ODYSSEY, FileData);
        if (!msbt.IsValid()) {
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