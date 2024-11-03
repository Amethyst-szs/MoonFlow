using Godot;
using System;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace Nindot.UnitTest;

public class UnitTestMsbtUSen : UnitTestMsbtSMOParse
{
    protected SarcResource SystemMessage = null;
    protected SarcResource StageMessage = null;
    protected SarcResource LayoutMessage = null;

    protected enum GameVersion {
        v100,
        v130
    };

    public override void SetupTest()
    {
    }

    public override UnitTestResult Test()
    {
        // Create sarc data
        UnitTestResult res = ReadSarcList(GameVersion.v100, "USen");
        if (res != UnitTestResult.OK)
            return res;

        // Iterate through every sarc to test each msbt
        res = ScanSarcMsbt(SystemMessage);
        if (res != UnitTestResult.OK)
            return res;
        
        res = ScanSarcMsbt(StageMessage);
        if (res != UnitTestResult.OK)
            return res;
        
        res = ScanSarcMsbt(LayoutMessage);
        if (res != UnitTestResult.OK)
            return res;

        return UnitTestResult.OK;
    }

    protected virtual UnitTestResult ReadSarcList(GameVersion ver, string lang)
    {
        // Ensure we have a path to the romfs
        string path;
        
        if (ver == GameVersion.v100) {
            path = (string)ProjectSettings.GetSetting(UnitTester._100PathKey, "");
        } else {
            path = (string)ProjectSettings.GetSetting(UnitTester._130PathKey, "");
        }
        
        if (path.Length == 0)
            return UnitTestResult.SKIP;
        
        // Append txt path to romfs path
        path += string.Format("LocalizedData/{0}/MessageData/", lang);

        // Read in all three sarcs
        SystemMessage = SarcResource.FromFilePath(path + "SystemMessage.szs");
        if (SystemMessage == null || SystemMessage.SarcDict.Count == 0) {
            GD.PrintErr("Failed to initalize SystemMessage.szs for unit test!");
            return UnitTestResult.FAILURE;
        }

        StageMessage = SarcResource.FromFilePath(path + "StageMessage.szs");
        if (StageMessage == null || StageMessage.SarcDict.Count == 0) {
            GD.PrintErr("Failed to initalize StageMessage.szs for unit test!");
            return UnitTestResult.FAILURE;
        }

        LayoutMessage = SarcResource.FromFilePath(path + "LayoutMessage.szs");
        if (LayoutMessage == null || LayoutMessage.SarcDict.Count == 0) {
            GD.PrintErr("Failed to initalize LayoutMessage.szs for unit test!");
            return UnitTestResult.FAILURE;
        }

        return UnitTestResult.OK;
    }

    protected virtual UnitTestResult ScanSarcMsbt(SarcResource sarc)
    {
        foreach (var x in sarc.SarcDict)
        {
            if (!x.Key.Contains(".msbt")) {
                GD.PushWarning("SystemMessage contains non-msbt file? Skipping file...");
                continue;
            }

            #if UNIT_TEST_VERBOSE
            GD.Print("Parsing " + x.Key);
            #endif

            byte[] file = [.. x.Value];

            MsbtFile msbt = new(TagLibraryHolder.Type.SUPER_MARIO_ODYSSEY, file);
            if (msbt == null || !msbt.IsValid()) {
                GD.PrintErr(string.Format("Failed to read MSBT - {0}", x.Key));
                return UnitTestResult.FAILURE;
            }

            UnitTestResult res = ScanElements(msbt.Content, false);
            if (res != UnitTestResult.OK)
                return res;
        }

        return UnitTestResult.OK;
    }

    public override void CleanupTest()
    {
        SystemMessage = null;
        StageMessage = null;
        LayoutMessage = null;
    }

    public override void Failure()
    {
    }
}