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

        file.Color_AddNew("Custom Color", 69, 69, 69, 69);
        foreach (var color in file.Color_GetLabelList())
        {
            var c = file.Color_Get(color);
            file.Color_MoveIndex(color, 0);
            continue;
        }
        file.Color_Remove("Custom Color");

        foreach (var attribute in file.Attribute_GetList())
        {
            var arrayContent = file.Attribute_GetContentArrayList(attribute);
            continue;
        }

        foreach (var group in file.TagGroup_GetList())
        {
            var tags = file.Tag_GetList(group);
            foreach (var tag in tags)
            {
                var paras = file.TagParam_GetList(tag);
                continue;
            }
        }

        foreach (var style in file.Style_GetLabelList())
        {
            var styleInfo = file.Style_Get(style);
            continue;
        }

        var project = file.Project_GetContent();
        file.Project_AddElement("123456789");
        file.Project_RemoveElement("Viewer/QuickSearch/NPCballoon.msqry");

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