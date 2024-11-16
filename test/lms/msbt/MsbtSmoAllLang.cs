#if TOOLS

namespace Nindot.UnitTest;

public class UnitTestMsbtAllLang : IUnitTest
{
    static private SarcResource SystemMessage = null;
    static private SarcResource StageMessage = null;
    static private SarcResource LayoutMessage = null;

    private static readonly string[] LangList = [
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

    public static void SetupTest()
    {
    }

    public static void RunTest()
    {
        var ver = UnitTestMsbtUSen.GameVersion.v130;

        foreach (var lang in LangList)
        {
            UnitTestMsbtUSen.ReadSarcList(ver, lang, out SystemMessage, out StageMessage, out LayoutMessage);
            UnitTestMsbtUSen.ScanSarcMsbt(SystemMessage);
            UnitTestMsbtUSen.ScanSarcMsbt(StageMessage);
            UnitTestMsbtUSen.ScanSarcMsbt(LayoutMessage);
        }
    }

    public static void CleanupTest()
    {
        SystemMessage = null;
        StageMessage = null;
        LayoutMessage = null;
    }
}

#endif