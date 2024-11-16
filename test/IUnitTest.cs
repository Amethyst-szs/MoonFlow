#if TOOLS

namespace Nindot.UnitTest;

public interface IUnitTest
{
    public static abstract void SetupTest();
    public static abstract void RunTest();
    public static abstract void CleanupTest();
}

#endif