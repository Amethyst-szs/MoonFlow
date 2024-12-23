using System;
using System.Collections.Generic;
using Godot;

using Nindot.LMS.Msbt.TagLib;

namespace MoonFlow.Scene.EditorMsbt;

public partial class MsbtPageEditor : TextEdit
{
    public override void _GuiInput(InputEvent @event)
    {
        if (@event.GetType() == typeof(InputEventMouseButton))
        {
            var m = (InputEventMouseButton)@event;
            GuiInputTrySpawnTagWheel(m);
            GuiInputTryOpenTagEdit(m);
        }

        // Only proceed if the event is an InputEventKey type
        if (@event.GetType() != typeof(InputEventKey)) return;
        var input = (InputEventKey)@event;

        if (input.IsActionPressed("ui_text_indent"))
        {
            GetViewport().SetInputAsHandled();
            return;
        }

        if (input.IsActionPressed("ui_text_newline", true))
        {
            _HandleUnicodeInput(Convert.ToChar('\n'), -1);
            GetViewport().SetInputAsHandled();
            return;
        }

        if (input.IsActionPressed("ui_text_delete", true))
        {
            SetCaretColumn(GetCaretColumn() + 1);
            _Backspace(0);
            GetViewport().SetInputAsHandled();
            return;
        }

        if (input.IsActionPressed("ui_undo", true, true))
        {
            Undo();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (input.IsActionPressed("ui_redo", true, true))
        {
            Redo();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (input.IsActionPressed("ui_add_tag", false, true))
        {
            SpawnTagWheel(GetCaretLine(), GetCaretColumn(), false);
            GetViewport().SetInputAsHandled();
            return;
        }

        if (input.IsActionPressed("ui_edit_tag", false, true))
        {
            var line = GetCaretLine();
            var col = GetCaretColumn();
            var pos = (Vector2I)GetCaretDrawPos();

            TryOpenTagEdit(GetCharIndex(line, col), pos);
        }
    }

    private void GuiInputTrySpawnTagWheel(InputEventMouseButton m)
    {
        if (!m.Pressed || m.ButtonIndex != MouseButton.Right || !Editable)
            return;

        var caretPos = GetLineColumnAtPos((Vector2I)GetLocalMousePos());
        SetCaretLine(caretPos.Y);
        SetCaretColumn(caretPos.X);

        SpawnTagWheel(caretPos.Y, caretPos.X, true);
        GetViewport().SetInputAsHandled();
    }

    private void GuiInputTryOpenTagEdit(InputEventMouseButton m)
    {
        if (!m.Pressed || m.ButtonIndex != MouseButton.Left || !Editable || !m.IsCommandOrControlPressed())
            return;

        // Get character index from mouse click position
        var mousePos = (Vector2I)GetLocalMousePos();
        var lc = GetLineColumnAtPos(mousePos);

        var rect = GetRectAtLineColumn(lc.Y, lc.X);
        bool isAddOne = mousePos.X - rect.Position.X > rect.Size.X;

        var charIdx = GetCharIndex(lc.Y, lc.X) + (isAddOne ? 1 : 0);

        if (TryOpenTagEdit(charIdx, mousePos))
            GetViewport().SetInputAsHandled();
    }

    public override void _HandleUnicodeInput(int unicodeChar, int caretIndex)
    {
        if (caretIndex == -1) caretIndex = 0;

        // If there is currently a selection, run backspace before inputting text
        if (HasSelection(caretIndex)) _Backspace(caretIndex);

        int line = GetCaretLine(caretIndex);
        int col = GetCaretColumn(caretIndex);
        int charIdx = GetCharIndex(line, col);

        string str = Convert.ToChar(unicodeChar).ToString();
        Page.InsertString(charIdx, str);
        InsertText(str, line, col);

        AdjustViewportToCaret(caretIndex);
        ActivityTimer.Start();
    }

    public void HandleTagInput(MsbtTagElement tag, int caretIndex)
    {
        if (caretIndex == -1) caretIndex = 0;

        // If there is currently a selection, run backspace before inputting text
        if (HasSelection(caretIndex)) _Backspace(caretIndex);

        int line = GetCaretLine(caretIndex);
        int col = GetCaretColumn(caretIndex);
        int charIdx = GetCharIndex(line, col);

        Page.InsertTag(charIdx, tag);
        InsertText("\u2E3A", line, col);

        AdjustViewportToCaret(caretIndex);
        ActivityTimer.Start();

        SyntaxHighlighter.ClearHighlightingCache();
    }

    public void DeleteViaContextMenu() { if (HasSelection()) _Backspace(-1); }
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

            AdjustViewportToCaret(caretIndex);
            ActivityTimer.Start();
            return;
        }

        int line = GetCaretLine(caretIndex);
        int col = GetCaretColumn(caretIndex) - 1;
        int charIdx = GetCharIndex(line, col);

        Page.Backspace(charIdx);

        if (col != -1)
        {
            RemoveText(line, col, line, col + 1);
        }
        else
        {
            if (line == 0) return;

            var lineStr = GetLine(line);
            var prevStr = GetLine(line - 1);
            var endingCaretColumn = prevStr.Length;

            RemoveLineAt(line);
            SetLine(line - 1, prevStr + lineStr);

            SetCaretLine(line - 1, true, true, 0, caretIndex);
            SetCaretColumn(endingCaretColumn, true, caretIndex);
        }

        AdjustViewportToCaret(caretIndex);
        ActivityTimer.Start();
    }

    public void CopyViaContextMenu() { _Copy(-1); }
    public override void _Copy(int caretIndex)
    {
        if (caretIndex == -1) caretIndex = 0;
        if (!HasSelection(caretIndex)) return;

        int start = GetCharIndex(GetSelectionFromLine(caretIndex), GetSelectionFromColumn(caretIndex));
        int end = GetCharIndex(GetSelectionToLine(caretIndex), GetSelectionToColumn(caretIndex));
        MsbtClipboardServer.Copy(Page, start, end);
    }

    public void CutViaContextMenu() { _Cut(-1); }
    public override void _Cut(int caretIndex)
    {
        if (caretIndex == -1) caretIndex = 0;
        if (!HasSelection(caretIndex)) return;

        _Copy(caretIndex);

        // Handles removing the text from both the TextEdit and page
        _Backspace(caretIndex);
    }

    public void PasteViaContextMenu() { _Paste(-1); }
    public override void _Paste(int caretIndex)
    {
        if (caretIndex == -1) caretIndex = 0;

        // Handles removing the text from both the TextEdit and page
        if (HasSelection(caretIndex)) _Backspace(caretIndex);

        MsbtClipboardServer.Paste(Page, GetCharIndex(caretIndex));
        InsertTextAtCaret(MsbtClipboardServer.GetClipboardAsString(), caretIndex);

        AdjustViewportToCaret(caretIndex);
        ActivityTimer.Start();

        SyntaxHighlighter.ClearHighlightingCache();
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return false;
    }

    public int GetCharIndex(int caretIndex)
    {
        int line = GetCaretLine(caretIndex);
        int col = GetCaretColumn(caretIndex);
        return GetCharIndex(line, col);
    }
    public int GetCharIndex(int line, int col)
    {
        int charIdx = 0;
        for (int i = 0; i < line; i++)
        {
            var str = GetLine(i);
            charIdx += str.Length + 1;
        }

        charIdx += col;
        return charIdx;
    }
}
