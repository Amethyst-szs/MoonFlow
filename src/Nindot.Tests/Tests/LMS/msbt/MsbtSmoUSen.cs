using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace Nindot.UnitTest;

public class UnitTestMsbtUSen : IUnitTest
{
    static private SarcFile SystemMessage = null;
    static private SarcFile StageMessage = null;
    static private SarcFile LayoutMessage = null;

    public static void SetupTest()
    {
        ReadSarcList("USen", out SystemMessage, out StageMessage, out LayoutMessage);
    }

    public static void RunTest()
    {
        ScanSarcMsbt(SystemMessage);
        ScanSarcMsbt(StageMessage);
        ScanSarcMsbt(LayoutMessage);
    }

    public static void ReadSarcList(string lang, out SarcFile system, out SarcFile stage, out SarcFile layout)
    {
        // Append txt path to romfs path
        var path = string.Format("{0}/LocalizedData/{1}/MessageData/", Test.RomfsDirectory, lang);

        // Read in all three sarcs
        system = SarcFile.FromFilePath(path + "SystemMessage.szs");
        Test.ShouldNot(system, null);
        Test.ShouldNot(system.GetFileCount(), 0);

        stage = SarcFile.FromFilePath(path + "StageMessage.szs");
        Test.ShouldNot(stage, null);
        Test.ShouldNot(stage.GetFileCount(), 0);

        layout = SarcFile.FromFilePath(path + "LayoutMessage.szs");
        Test.ShouldNot(layout, null);
        Test.ShouldNot(layout.GetFileCount(), 0);
    }

    public static void ScanSarcMsbt(SarcFile sarc)
    {
        foreach (var x in sarc.GetFileList())
        {
            Test.Should(x.Contains(".msbt"));

            byte[] file = sarc.GetFile(x);
            MsbtFile msbt = new(new MsbtElementFactoryProjectSmo(), file);
            Test.Should(msbt.IsValid());

            // UnitTestMsbtSMOParse.TestAllElements(msbt.Content);
        }
    }

    public static void CleanupTest()
    {
        SystemMessage = null;
        StageMessage = null;
        LayoutMessage = null;
    }

    public static void Failure()
    {
    }
}