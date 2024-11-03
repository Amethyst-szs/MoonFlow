#if TOOLS
using Godot;
using System;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;
using Nindot.LMS.Msbp;

namespace Nindot.UnitTest;

public class UnitTestMsbtV2SMOWrite : UnitTestBase
{
    protected byte[] FileData = [];

    public override void SetupTest()
    {
        FileData = FileAccess.GetFileAsBytes("res://unit_test/msbt/AmiiboNpc.msbt");
    }

    public override UnitTestResult Test()
    {
        // Load in an msbt resource and check it's validity
        MsbtFile msbt = new(TagLibraryHolder.Type.SUPER_MARIO_ODYSSEY, FileData);
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

        FileAccess writer = FileAccess.Open("user://AmiiboNpcOUTPUT.msbt", FileAccess.ModeFlags.Write);
        writer.StoreBuffer(stream.ToArray());
        writer.Close();

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