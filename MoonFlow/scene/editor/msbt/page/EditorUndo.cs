using System.Collections.Generic;
using Godot;
using Nindot.LMS.Msbt;

namespace MoonFlow.Scene.EditorMsbt;

public partial class MsbtPageEditor : TextEdit
{
    private readonly struct UndoEntry(MsbtPage originalPage, int caretLine, int caretColumn)
    {
        public readonly int CaretLine = caretLine;
        public readonly int CaretColumn = caretColumn;
        public readonly MsbtPage PageClone = originalPage.Clone();
    }

    private readonly List<UndoEntry> UndoTree = [];
    private int UndoTreePosition = 1;
    private const int UndoTreeMax = 25;

    private void RegisterUndoEntry()
    {
        // This undo system completely replaces Godot's TextEdit undo/redo system
        // Since there is no way to disable the original undo/redo implementation fully,
        // just clear out the original impl's history here to avoid it filling up with wasted data
        ClearUndoHistory();

        // If currently navigating the past, eliminate the future >:)
        if (UndoTreePosition > 1)
        {
            UndoTree.RemoveRange(UndoTree.Count - UndoTreePosition, UndoTreePosition - 1);
            UndoTreePosition = 1;
        }

        // Add new undo tree element
        UndoTree.Add(new UndoEntry(Page, GetCaretLine(), GetCaretColumn()));

        // If undo tree is larger than the maximum size, remove the oldest entry
        if (UndoTree.Count > UndoTreeMax)
            UndoTree.RemoveAt(0);
    }

    public new void Undo()
    {
        // If the activity time is running, perform an early undo entry
        if (!ActivityTimer.IsStopped())
            RegisterUndoEntry();

        // Return early if undo tree is unprepared
        if (UndoTree.Count < 2) return;
        if (UndoTreePosition >= UndoTree.Count) return;

        // Get next point in the undo tree and prepare
        UndoTreePosition++;
        var d = UndoTree[^UndoTreePosition];

        Page = d.PageClone;
        ReloadTextEdit();
        SetCaretLine(d.CaretLine);
        SetCaretColumn(d.CaretColumn);
    }

    public new void Redo()
    {
        // Return if there is nothing to redo
        if (UndoTreePosition <= 1) return;

        // Get previous point in the undo tree and prepare
        UndoTreePosition--;
        var d = UndoTree[^UndoTreePosition];

        Page = d.PageClone;
        ReloadTextEdit();
        SetCaretLine(d.CaretLine);
        SetCaretColumn(d.CaretColumn);
    }

    public new bool HasUndo()
    {
        return UndoTree.Count > 1 && UndoTreePosition < UndoTree.Count;
    }
    public new bool HasRedo()
    {
        return UndoTreePosition > 1;
    }

    private void ActivityTimerTimeout()
    {
        RegisterUndoEntry();
    }
}
