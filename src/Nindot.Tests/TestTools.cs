using System;

namespace Nindot.UnitTest;

public class UnitTestException : Exception;

public static class Test
{
    public const string TestOutputDirectory = "./src/Nindot.Tests/output/";
    public static string RomfsDirectory { get; internal set; } = null;
    public static string GameName { get; internal set; } = null;

    public static bool IsValidRomfsDirectory() { return RomfsDirectory != null && RomfsDirectory != "None"; }
    public static bool IsValidGameName() { return GameName != null && GameName != "None"; }

    public static void Should(bool a) { if (!a) throw new UnitTestException(); }
    public static void Should(object a, object b) { if (!a.Equals(b)) throw new UnitTestException(); }
    public static void ShouldNot(bool a) { if (a) throw new UnitTestException(); }
    public static void ShouldNot(object a, object b) { if (a.Equals(b)) throw new UnitTestException(); }
}