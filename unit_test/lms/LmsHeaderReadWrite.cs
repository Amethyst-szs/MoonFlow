#if TOOLS
using Godot;
using System;
using System.IO;

namespace Nindot.UnitTest;

public class UnitTestLmsHeaderReadWrite : UnitTestBase
{
    protected byte[] FileData = [];

    public override void SetupTest()
    {
        FileData = Godot.FileAccess.GetFileAsBytes("res://unit_test/msbt/SmoUnitTesting.msbt");
    }

    public override UnitTestResult Test()
    {
        LMS.FileHeader head = new(FileData);
        if (!head.IsValid())
            return UnitTestResult.FAILURE;
        
        MemoryStream stream = new();
        if (!head.WriteHeader(ref stream))
            return UnitTestResult.FAILURE;
        
        return UnitTestResult.OK;
    }

    public override void CleanupTest()
    {
        FileData = null;
    }

    public override void Failure()
    {
    }
}

#endif