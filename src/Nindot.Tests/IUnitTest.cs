namespace Nindot.UnitTest;

public interface IUnitTestGroup
{
    public static abstract void SetupGroup();
    public static abstract void CleanupGroup();
}

// Flag a method in an IUnitTest as a method to run during testing! Must be static and public
[System.AttributeUsage(System.AttributeTargets.Method)]
public class RunTest : System.Attribute;

// Flag a RunTest method as requiring the game value to be set to "SMO" and to have a valid romfs path
[System.AttributeUsage(System.AttributeTargets.Method)]
public class SmoRomfsTest : System.Attribute;