using Godot;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;

namespace MoonFlow.Scene.EditorMsbt;

[GlobalClass]
public partial class MsbtPageEditor : TextEdit
{
    public SarcMsbpFile Project = null;
    public MsbtPage Page = null;

    public Timer ActivityTimer = new();

    [Signal]
    public delegate void PageModifiedEventHandler();

    public override void _Ready()
    {
        // Setup activity timer
        ActivityTimer.Autostart = false;
        ActivityTimer.OneShot = true;
        ActivityTimer.WaitTime = 0.45;
        ActivityTimer.Timeout += ActivityTimerTimeout;
        AddChild(ActivityTimer);
    }

    public override void _ExitTree()
    {
        if (IsInstanceValid(ActivityTimer))
            ActivityTimer.QueueFree();
    }

    public MsbtPageEditor Init(SarcMsbpFile project, MsbtPage page)
    {
        // Ensure page is assigned to a value (can be passed as null)
        bool isPagePassedAsNull = page == null;
        page ??= [];
        
        Project = project;
        Page = page;

        PrepareTextEdit();

        // If page is passed as null, assign placeholder text
        if (isPagePassedAsNull)
            PlaceholderText = Tr("NullPage", "MSBT_PAGE_EDITOR");
        
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
