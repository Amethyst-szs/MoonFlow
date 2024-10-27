#if TOOLS
using Godot;
using System;

using Nindot.MsbtContent;
using Nindot.MsbtTagLibrary;
using Nindot.MsbtTagLibrary.Smo;

namespace Nindot.UnitTest;

public class UnitTestMsbtAllLang : UnitTestMsbtUSen
{
    static readonly string[] LangList = [
        "CNzh",
        "EUde",
        "EUen",
        "EUes",
        "EUfr",
        "EUit",
        "EUnl",
        "EUru",
        "JPja",
        "TWzh",
        "USen",
        "USes",
        "USfr",
    ];

    public override void SetupTest()
    {
    }

    public override UnitTestResult Test()
    {
        foreach (var lang in LangList) {
        // Create sarc data
            UnitTestResult res = ReadSarcList(GameVersion.v100, lang);
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

#endif