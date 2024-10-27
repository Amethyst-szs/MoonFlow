#if TOOLS
using Godot;
using System;

namespace Nindot.UnitTest;

public class UnitTestBase
{
    public virtual void SetupTest()
    {
    }

    public virtual UnitTestResult Test()
    {
        return UnitTestResult.OK;
    }

    public virtual void CleanupTest()
    {
    }

    public virtual void Failure()
    {
    }

    public virtual bool IsRequireRomFs()
    {
        return false;
    }

    public void Print(UnitTestStatusCode statusCode, string msg)
    {
        GD.Print(string.Format("UnitTest > {0} > {1} : {2}", statusCode.ToString(), GetType(), msg));
    }

    public void PrintWarn(string msg)
    {
        GD.PushWarning(string.Format("UnitTest > {0} > {1} : {2}", UnitTestStatusCode.Warn.ToString(), GetType(), msg));
    }

    public void PrintErr(string msg)
    {
        GD.PushError(string.Format("UnitTest > {0} > {1} : {2}", UnitTestStatusCode.Error.ToString(), GetType(), msg));
    }

    public void PrintTestStarted()
    {
        Print(UnitTestStatusCode.Begin, "Test Starting...");
    }

    public void PrintTestSuccess()
    {
        Print(UnitTestStatusCode.Success, "Test Completed Successfully!");
    }

    public void PrintTestSkipped()
    {
        Print(UnitTestStatusCode.Skip, "Test Skipped.");
    }

    public void PrintTestSkippedRomFs()
    {
        Print(UnitTestStatusCode.Skip, "Set romfs paths in project settings to run this test");
    }

    public void PrintTestError()
    {
        Print(UnitTestStatusCode.Error, "! - Test Failed - !");
    }
}

public enum UnitTestResult
{
    OK,
    SKIP,
    FAILURE,
}

public class UnitTestStatusCode
{
    private UnitTestStatusCode(string value) { Value = value; }

    public string Value { get; private set; }

    public static UnitTestStatusCode Begin { get { return new UnitTestStatusCode("SRT"); } }
    public static UnitTestStatusCode Success { get { return new UnitTestStatusCode(" OK"); } }
    public static UnitTestStatusCode Skip { get { return new UnitTestStatusCode(" SK"); } }
    public static UnitTestStatusCode Warn { get { return new UnitTestStatusCode("WRN"); } }
    public static UnitTestStatusCode Error { get { return new UnitTestStatusCode("ERR"); } }

    public override string ToString()
    {
        return Value;
    }
}

#endif