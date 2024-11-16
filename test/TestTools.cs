#if TOOLS

using System;

namespace Nindot.UnitTest;

public class UnitTestException : Exception;

public static class Test
{
    public static void Should(bool a) { if (!a) throw new UnitTestException(); }
    public static void Should(object a, object b) { if (!a.Equals(b)) throw new UnitTestException(); }
    public static void ShouldNot(bool a) { if (a) throw new UnitTestException(); }
    public static void ShouldNot(object a, object b) { if (a.Equals(b)) throw new UnitTestException(); }
}

#endif