#if TOOLS
using Godot;
using System;
using System.IO;

namespace Nindot.UnitTest;

public class UnitTestMsbp3DWRead : UnitTestBase
{
    protected byte[] FileData = [];

    public override void SetupTest()
    {
        FileData = Godot.FileAccess.GetFileAsBytes("res://unit_test/lms/msbp/ProjectData-3DW-BF.msbp");
    }

    public override UnitTestResult Test()
    {
        LMS.Msbp.MsbpFile file = new(FileData);
        if (!file.IsValid())
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