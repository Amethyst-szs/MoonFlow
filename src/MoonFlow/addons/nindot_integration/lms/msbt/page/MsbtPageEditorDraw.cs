using Godot;

namespace MoonFlow.LMS.Msbt;

public partial class MsbtPageEditor : TextEdit
{
    public override void _Draw()
    {
        foreach (var item in Page)
        {
            if (item.IsText())
                continue;

            // Calculate the rect of this tag's glyph
            int charIdx = Page.CalcCharPosForElement(item);
            CalcLineColumnAtCharIdx(charIdx, out int line, out int col);
            Rect2I glyphRect = GetRectAtLineColumn(line, col);

            // Conform glyph to 1:1 aspect ratio
            int sizeDif = glyphRect.Size.Y - glyphRect.Size.X;
            glyphRect = new(glyphRect.Position.X + (sizeDif / 2), glyphRect.Position.Y + (sizeDif / 2),
                glyphRect.Size.X, glyphRect.Size.X);

            if (item.IsTag())
                DrawTextureRect(TestTex, glyphRect, false, null);
            
            if (item.IsTagClose())
                DrawTextureRect(TestTex, glyphRect, false, null, true);
        }

    }

    private void CalcLineColumnAtCharIdx(int charIdx, out int line, out int col)
    {
        line = 0;
        col = 0;

        while (GetLineCount() > line)
        {
            string lineStr = GetLine(line);

            if (lineStr.Length <= charIdx)
            {
                line++;
                charIdx -= lineStr.Length;
                continue;
            }

            col = charIdx;
            return;
        }
    }
}
