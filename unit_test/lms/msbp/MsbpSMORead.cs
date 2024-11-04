#if TOOLS
using Godot;
using System;
using System.IO;

namespace Nindot.UnitTest;

public class UnitTestMsbpSMORead : UnitTestBase
{
    protected byte[] FileData = [];

    public override void SetupTest()
    {
        FileData = Godot.FileAccess.GetFileAsBytes("res://unit_test/lms/msbp/ProjectData-SMO.msbp");
    }

    public override UnitTestResult Test()
    {
        LMS.Msbp.MsbpFile file = new(FileData);
        if (!file.IsValid())
            return UnitTestResult.FAILURE;

        file.ColorAddNew("Custom Color", 69, 69, 69, 69);
        foreach (var color in file.ColorGetLabelList())
        {
            var c = file.ColorGet(color);
            file.ColorMoveIndex(color, 0);
            continue;
        }
        file.ColorRemove("Custom Color");

        foreach (var attribute in file.AttributeGetList())
        {
            var arrayContent = file.AttributeGetContentArrayList(attribute);
            continue;
        }

        foreach (var group in file.TagGroupGetList())
        {
            var tags = file.TagGetList(group);
            foreach (var tag in tags)
            {
                var paras = file.TagParamGetList(tag);
                continue;
            }
        }

        foreach (var style in file.StyleGetLabelList())
        {
            var styleInfo = file.StyleGet(style);
            continue;
        }

        var project = file.ProjectGetContent();
        file.ProjectAddElement("123456789");
        file.ProjectRemoveElement("Viewer/QuickSearch/NPCballoon.msqry");

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