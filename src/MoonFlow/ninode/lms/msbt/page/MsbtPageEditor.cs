using Godot;
using Godot.Collections;
using System;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;
using System.Text;
using MoonFlow.Project;

namespace MoonFlow.LMS.Msbt;

[GlobalClass]
public partial class MsbtPageEditor : TextEdit
{
    public SarcMsbpFile Project = null;
    public MsbtPage Page = null;

    public Timer ActivityTimer = new();

    public override void _Ready()
    {
        // Setup activity timer
        ActivityTimer.Autostart = false;
        ActivityTimer.OneShot = true;
        ActivityTimer.WaitTime = 0.45;
        ActivityTimer.Timeout += ActivityTimerTimeout;
        AddChild(ActivityTimer);
    }

    public MsbtPageEditor Init(SarcMsbpFile project, MsbtPage page)
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
}
