namespace Nindot.UnitTest;

public class UnitTestMsbtAllLang : IUnitTest
{
    static private SarcFile SystemMessage = null;
    static private SarcFile StageMessage = null;
    static private SarcFile LayoutMessage = null;

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
        foreach (var lang in LangList)
        {
            UnitTestMsbtUSen.ReadSarcList(lang, out SystemMessage, out StageMessage, out LayoutMessage);
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