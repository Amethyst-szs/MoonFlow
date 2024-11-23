using System;
using Godot;

namespace MoonFlow.LMS.Msbt;

public partial class MsbtPageEditor : TextEdit
{
    public override void _HandleUnicodeInput(int unicodeChar, int caretIndex)
    {
        if (caretIndex == -1) caretIndex = 0;

        int line = GetCaretLine(caretIndex);
        int col = GetCaretColumn(caretIndex);
        int charIdx = GetCharIndex(line, col);

        string str = Convert.ToChar(unicodeChar).ToString();
        Page.InsertString(charIdx, str);
        InsertText(str, line, col);
    }

    public override void _Backspace(int caretIndex)
    {
        if (caretIndex == -1) caretIndex = 0;

        if (HasSelection(caretIndex))
        {
            int startL = GetSelectionFromLine(caretIndex);
            int startC = GetSelectionFromColumn(caretIndex);
            int endL = GetSelectionToLine(caretIndex);
            int endC = GetSelectionToColumn(caretIndex);

            int start = GetCharIndex(startL, startC);
            int end = GetCharIndex(endL, endC);

            Page.BackspaceRange(start, end);
            RemoveText(startL, startC, endL, endC);
            return;
        }

        int line = GetCaretLine(caretIndex);
        int col = GetCaretColumn(caretIndex) - 1;
        int charIdx = GetCharIndex(line, col);

        SetLine(line, GetLine(line).Remove(col, 1));
        SetCaretColumn(col, true, caretIndex);

        Page.Backspace(charIdx);
    }

    public override void _Copy(int caretIndex)
    {
        if (caretIndex == -1) caretIndex = 0;

        int start = GetCharIndex(GetSelectionFromLine(caretIndex), GetSelectionFromColumn(caretIndex));
        int end = GetCharIndex(GetSelectionToLine(caretIndex), GetSelectionToColumn(caretIndex));
        MsbtClipboardServer.Copy(Page, start, end);
    }

    public override void _Cut(int caretIndex)
    {
        if (caretIndex == -1) caretIndex = 0;

        _Copy(caretIndex);

        // Handles removing the text from both the TextEdit and page
        _Backspace(caretIndex);
    }

    public override void _Paste(int caretIndex)
    {
        if (caretIndex == -1) caretIndex = 0;

        // Handles removing the text from both the TextEdit and page
        if (HasSelection(caretIndex)) _Backspace(caretIndex);

        MsbtClipboardServer.Paste(Page, GetCharIndex(caretIndex));
        InsertTextAtCaret(MsbtClipboardServer.GetClipboardAsString(), caretIndex);
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return false;
    }

    public override void _ShortcutInput(InputEvent @event)
    {
        if (Input.IsActionJustReleased("ui_undo", true)) MsbtUndo();
        if (Input.IsActionJustReleased("ui_redo", true)) MsbtRedo();
    }

    public void MsbtUndo()
    {
        GD.Print("Undo shortcut");
    }

    public void MsbtRedo()
    {
        GD.Print("Redo shortcut");
    }
}
