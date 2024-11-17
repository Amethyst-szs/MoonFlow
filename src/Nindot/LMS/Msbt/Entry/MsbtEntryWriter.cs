using System;
using System.Linq;

using Nindot.LMS.Msbt.TagLib;

namespace Nindot.LMS.Msbt;

public partial class MsbtEntry
{
    private int Cursor = 0;
    private int CursorElement = 0;

    // ====================================================== //
    // ================== General Utilities ================= //
    // ====================================================== //

    public MsbtBaseElement GetCursorElement()
    {
        try { return Elements.ElementAt(CursorElement); }
        catch { return null; }
    }
    public int GetCursorElementLength()
    {
        var curElement = GetCursorElement();
        if (curElement == null) return 0;

        if (curElement.GetType() == typeof(MsbtTextElement))
            return ((MsbtTextElement)curElement).Text.Length;

        return 1;
    }

    // ====================================================== //
    // ================= Cursor Manipulation ================ //
    // ====================================================== //

    public void CursorNext() { CursorMoveOffset(1); }
    public void CursorBack() { CursorMoveOffset(-1); }
    public void CursorMoveOffset(int offset)
    {
        var curElement = GetCursorElement();
        if (curElement == null) return;

        int elementLength = GetCursorElementLength();

        // Add the offset request to the cursor
        Cursor += offset;

        // If the new position is under 0 on element 0, clamp
        // If over the element length of the final element, clamp
        if (Cursor < 0 && CursorElement == 0)
        {
            Cursor = 0;
            return;
        }
        else if (Cursor > elementLength && CursorElement == Elements.Count - 1)
        {
            Cursor = elementLength;
            return;
        }

        // If the cursor is out of bounds for the selected element, loop until valid
        while (CursorIsOutOfBounds())
        {
            elementLength = GetCursorElementLength();
            if (Cursor > elementLength)
            {
                Cursor -= elementLength;
                CursorElement += 1;
            }
            else if (Cursor < 0)
            {
                Cursor += elementLength;
                CursorElement -= 1;
            }

            CursorElement = Math.Clamp(CursorElement, 0, Elements.Count - 1);
        }
    }

    private bool CursorIsOutOfBounds()
    {
        if (Cursor < 0) return true;

        var curElement = GetCursorElement();
        if (curElement == null) return true;

        if (curElement.GetType() == typeof(MsbtTextElement))
            return Cursor > ((MsbtTextElement)curElement).Text.Length;

        return Cursor > 0;
    }
}