#if TOOLS
using Godot;
using System;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace Nindot.UnitTest;

public class UnitTestMsbtUSen : IUnitTest
{
    static private SarcResource SystemMessage = null;
    static private SarcResource StageMessage = null;
    static private SarcResource LayoutMessage = null;

    public enum GameVersion
    {
        v100,
        v130
    };

    public static void SetupTest()
    {
        ReadSarcList(GameVersion.v130, "USen", out SystemMessage, out StageMessage, out LayoutMessage);
    }

    public static void RunTest()
    {
        ScanSarcMsbt(SystemMessage);
        ScanSarcMsbt(StageMessage);
        ScanSarcMsbt(LayoutMessage);
    }

    public static void ReadSarcList(GameVersion ver, string lang, out SarcResource system, out SarcResource stage, out SarcResource layout)
    {
        // Ensure we have a path to the romfs
        string path;

        if (ver == GameVersion.v100)
        {
            path = (string)ProjectSettings.GetSetting(UnitTester._100PathKey, "");
        }
        else
        {
            path = (string)ProjectSettings.GetSetting(UnitTester._130PathKey, "");
        }

        Test.ShouldNot(path, string.Empty);

        // Append txt path to romfs path
        path += string.Format("LocalizedData/{0}/MessageData/", lang);

        // Read in all three sarcs
        system = SarcResource.FromFilePath(path + "SystemMessage.szs");
        Test.ShouldNot(system, null);
        Test.ShouldNot(system.GetFileCount(), 0);

        stage = SarcResource.FromFilePath(path + "StageMessage.szs");
        Test.ShouldNot(stage, null);
        Test.ShouldNot(stage.GetFileCount(), 0);

        layout = SarcResource.FromFilePath(path + "LayoutMessage.szs");
        Test.ShouldNot(layout, null);
        Test.ShouldNot(layout.GetFileCount(), 0);
    }

    public static void ScanSarcMsbt(SarcResource sarc)
    {
        foreach (var x in sarc.GetFileList())
        {
            Test.Should(x.Contains(".msbt"));

            byte[] file = sarc.GetFile(x);
            MsbtFile msbt = new(new MsbtElementFactoryProjectSmo(), file);
            Test.Should(msbt.IsValid());

            UnitTestMsbtSMOParse.TestAllElements(msbt.Content);
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
#endif