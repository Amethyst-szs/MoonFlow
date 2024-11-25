using Godot;
using Godot.Collections;
using System;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;
using System.Text;

namespace MoonFlow.LMS.Msbt;

[GlobalClass]
public partial class MsbtPageEditor : TextEdit
{
    public MsbpFile Project = null;
    public MsbtPage Page = null;

    public Timer ActivityTimer = new();

    private readonly static PackedScene TagWheel = GD.Load<PackedScene>("res://ninode/lms/msbt/tag/tag_wheel.tscn");

    public override void _Ready()
    {
        // Setup activity timer
        ActivityTimer.Autostart = false;
        ActivityTimer.OneShot = true;
        ActivityTimer.WaitTime = 0.33;
        ActivityTimer.Timeout += ActivityTimerTimeout;
        AddChild(ActivityTimer);

        // TODO: DEBUG BULLSHITTERY REMOVE LATER
        var proj = MsbpFile.FromBytes(FileAccess.GetFileAsBytes("res://example/msbt/ProjectData.msbp"));
        var file = MsbtFile.FromBytes(FileAccess.GetFileAsBytes("res://example/msbt/SmoUnitTesting.msbt"), new MsbtElementFactoryProjectSmo());
        var entry = file.GetEntry(2);
        var page = entry.Pages[0];

        page.Add(new MsbtTagElementPictureFont(TagNamePictureFont.COMMON_MARIO));
        page.Add(new MsbtTagElementPictureFont(TagNamePictureFont.COMMON_CAPPY));
        // page.Add(new MsbtTagElementDeviceFont(TagNameDeviceFont.Album));
        Init(proj, page);
    }

    public MsbtPageEditor Init(MsbpFile project, MsbtPage page)
    {
        Project = project;
        Page = page;

        PrepareTextEdit();
        return this;
    }

    private void PrepareTextEdit()
    {
        // Setup default properties
        AutowrapMode = TextServer.AutowrapMode.WordSmart;
        DragAndDropSelectionEnabled = false;
        MiddleMousePasteEnabled = false;
        WrapMode = LineWrappingMode.Boundary;
        ClipContents = false;
        ClipChildren = ClipChildrenMode.Disabled;

        // Setup syntax highlighter
        SyntaxHighlighter = new SyntaxHighlighterMsbtPage();

        // Disable right click context menu
        ContextMenuEnabled = false;

        // Setup text string to match page elements
        ReloadTextEdit();
        RegisterUndoEntry();
    }

    private void ReloadTextEdit()
    {
        BeginComplexOperation();
        Text = "";
        foreach (var item in Page)
        {
            if (item.IsText())
                Text += item.GetText();
            else
                Text += '\u2E3A';
        }
        EndComplexOperation();
    }

    private void ActivityTimerTimeout()
    {
        RegisterUndoEntry();
    }

    private void SpawnTagWheel(int line, int column)
    {
        Editable = false;

        // Calculate start of mouse line position
        var caretOrigin = GetRectAtLineColumn(line, column);
        var caretPos = caretOrigin.GetCenter();
        caretPos.X += caretOrigin.Size.X / 2;

        var wheel = (TagWheel)TagWheel.Instantiate();
        wheel.TreeExiting += CloseTagWheel;
        wheel.CaretPosition = caretPos;
        wheel.SetPosition((Vector2I)GetLocalMousePosition());

        // Add wheel as child of page editor
        AddChild(wheel);
    }

    private void CloseTagWheel()
    {
        Editable = true;
    }
}
