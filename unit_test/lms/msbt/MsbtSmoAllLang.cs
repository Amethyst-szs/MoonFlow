#if TOOLS
using Godot;
using System;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

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
        "USes",
        "USfr",
    ];

    public string LastLang = "";

    public override void SetupTest()
    {
    }

    public override UnitTestResult Test()
    {
        foreach (var lang in LangList)
        {
            LastLang = lang;

            // Create sarc data
            UnitTestResult res = ReadSarcList(GameVersion.v130, lang);
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

        LastLang = "";
        return UnitTestResult.OK;
    }

    public override void CleanupTest()
    {
        SystemMessage = null;
        StageMessage = null;
        LayoutMessage = null;

        if (LastLang.Length != 0)
            GD.Print("Failure Language: " + LastLang);
    }

    public override void Failure()
    {
    }
}
#endif