using Godot;
using Godot.Collections;
using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.LMS.Msbt;

[GlobalClass]
public partial class SyntaxHighlighterMsbtPage : SyntaxHighlighter
{
    private static readonly Dictionary TextDefault = new(){{"color", new Color(1, 1, 1)}};
    private static readonly Dictionary Tag = new(){{"color", new Color(0, 0, 0, 0)}};

    private Dictionary TextColor = TextDefault;

    private Dictionary<int, Dictionary> LineFinalColor = [];

    public override Dictionary _GetLineSyntaxHighlighting(int line)
    {
        // If rendering the first line, reset text color
        if (line == 0) TextColor = TextDefault;

        // Get initial text color from last entry in LineFinalColor
        for (int i = line - 1; i >= 0; i--)
        {
            if (!LineFinalColor.ContainsKey(i))
                continue;
            
            TextColor = LineFinalColor[i];
            break;
        }

        // Setup result and local vars
        Dictionary<int, Dictionary> result = new(){
            {0, TextColor}
        };

        var edit = (MsbtPageEditor)GetTextEdit();
        var str = edit.GetLine(line);
        int baseCharIdx = edit.GetCharIndex(line, 0);

        bool isLastColumnText = true;

        for (int column = 0; column < str.Length; column++)
        {
            // Get access to the element at this column
            int charIdx = baseCharIdx + column;
            int elementIdx = edit.Page.CalcElementIdxAtCharPos(ref charIdx);
            MsbtBaseElement e = edit.Page[elementIdx];

            // If this element is a color tag, change text color
            if (e.GetType() == typeof(MsbtTagElementSystemColor) && edit.Project != null)
            {
                var colorTag = (MsbtTagElementSystemColor)e;
                colorTag.GetColor(edit.Project, out BlockColor.Entry c, out string _);
                TextColor = new(){{"color", Color.Color8(c.R, c.G, c.B, c.A)}};
            }

            // Assign the current column's color
            if (e.IsText() && !isLastColumnText)
            {
                result[column] = TextColor;
                isLastColumnText = true;
            }
            else if (!e.IsText() && isLastColumnText)
            {
                result[column] = Tag;
                isLastColumnText = false;
            }
        }

        LineFinalColor[line] = TextColor;

        return (Dictionary)result;
    }

    public override void _ClearHighlightingCache()
    {
        TextColor = TextDefault;
        LineFinalColor.Clear();
    }
}