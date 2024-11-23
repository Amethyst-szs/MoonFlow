using Godot;

namespace MoonFlow.LMS.Msbt;

public partial class MsbtPageEditor : TextEdit
{
    public int GetCharIndex(int caretIndex)
    {
        int line = GetCaretLine(caretIndex);
        int col = GetCaretColumn(caretIndex);
        return GetCharIndex(line, col);
    }
    public int GetCharIndex(int line, int col)
    {
        int idx = 0;
        for (int i = 0; i < GetLineCount() - 1; i++)
            idx += GetLine(line).Length;

        idx += col;
        return idx;
    }
}
