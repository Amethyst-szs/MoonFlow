using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Nindot.LMS.Msbt.TagLib;

namespace Nindot.LMS.Msbt;

public class MsbtPage : List<MsbtBaseElement>
{
    public MsbtPage() { }
    public MsbtPage(MsbtBaseElement firstElement)
    {
        Add(firstElement);
    }

    public MsbtPage Clone()
    {
        var page = new MsbtPage();
        foreach (var item in this) page.Add(item.Clone());
        return page;
    }

    public void WriteBytes(MemoryStream stream)
    {
        foreach (var item in this) { item.WriteBytes(stream); }
    }

    public void Cleanup()
    {
        // Iterate through all elements
        int elementIdx = 0;
        while (elementIdx < Count - 1)
        {
            // Get the current element and skip forward if it isn't a text element
            MsbtBaseElement cur = this[elementIdx];
            if (cur.GetType() != typeof(MsbtTextElement))
            {
                elementIdx++;
                continue;
            }

            MsbtTextElement curT = (MsbtTextElement)cur;
            curT.RemoveNullTerminator();

            // Delete text element if completely empty
            if (curT.Text.Length == 0)
            {
                Remove(curT);
                continue;
            }

            // Attempt to merge neighboring text elements if they exist
            MsbtBaseElement next = this[elementIdx + 1];
            if (next.GetType() == typeof(MsbtTextElement))
            {
                MsbtTextElement nextT = (MsbtTextElement)next;
                curT.Text += nextT.Text;

                Remove(next);
                continue;
            }

            // Advance to next element
            elementIdx++;
        }
    }

    // ====================================================== //
    // =================== Extra Utilities ================== //
    // ====================================================== //

    public void InsertString(int position, string str)
    {
        // Get access to the element at this position
        int elementIdx = CalcElementIdxAtCharPos(ref position);

        // If the element index is out of bounds, create a new text element and return
        if (elementIdx >= Count)
        {
            Insert(elementIdx, new MsbtTextElement(str));
            return;
        }

        var curElement = this[elementIdx];

        // If the current element isn't text, and the position is advanced past position 0, create
        // a new text element to store the insert into
        if (!curElement.IsText() && position > 0)
        {
            Add(new MsbtTextElement(str));
            return;
        }

        // If this element isn't a text element (when the cursor is right on the start of a tag)
        // create a new element and insert it at this position, then run cleanup
        if (!curElement.IsText())
        {
            Insert(elementIdx, new MsbtTextElement(str));
            Cleanup();
            return;
        }

        var txtElement = (MsbtTextElement)curElement;
        txtElement.Text = txtElement.Text.Insert(position, str);
    }

    public void InsertTag(int position, MsbtTagElement tag)
    {
        // Get access to the element at this position
        int elementIdx = CalcElementIdxAtCharPos(ref position);
        var curElement = this[elementIdx];

        // If the element index is beyond the final element and that element is a tag, just append tag to end of list
        if (curElement == this.Last() && position > 0 && !this.Last().IsText())
        {
            Add(tag);
            return;
        }

        // If the current element is text, split in half at the position dividing point and insert in middle
        if (curElement.IsText())
        {
            var txt = (MsbtTextElement)curElement;
            string substr1 = txt.Text[..position];
            string substr2 = txt.Text[position..];

            txt.Text = substr1;
            Insert(elementIdx + 1, new MsbtTextElement(substr2));
            Insert(elementIdx + 1, tag);

            Cleanup();
            return;
        }

        // Otherwise, just insert tag at position
        Insert(elementIdx, tag);
    }

    public void Backspace(int position)
    {
        // Get access to the element at this position
        int elementIdx = CalcElementIdxAtCharPos(ref position);
        var curElement = this[elementIdx];

        // If not working with a text element, just delete the element
        if (!curElement.IsText())
        {
            Remove(curElement);

            // Run the cleanup routine to merge neighbor text elements
            Cleanup();

            return;
        }

        // Otherwise, backspace one character and delete if string is now empty
        var txtElement = (MsbtTextElement)curElement;
        txtElement.Text = txtElement.Text.Remove(position, 1);

        if (txtElement.Text.Length == 0)
            Remove(curElement);

        // If the page is completely empty with no elements, add an empty text element
        if (Count == 0) Add(new MsbtTextElement(""));

        // Run the cleanup routine to merge neighbor text elements
        Cleanup();
    }

    public void BackspaceRange(int start, int end)
    {
        // Setup variables to track progress through backspacing the range
        int delPos = 0;
        int delLength = end - start;
        int elementIdx = CalcElementIdxAtCharPos(ref start);

        // Loop through all elements starts at the initial elementIdx and start deleting data
        while (delPos < delLength)
        {
            MsbtBaseElement item = this[elementIdx];

            int itemLength = 1;
            if (item.IsText())
                itemLength = item.GetText().Length;

            // If the item length is less than the distance to delLength, delete entire element
            if (start == 0 && itemLength - start < delLength - delPos)
            {
                Remove(item);

                start = 0;
                delPos += itemLength;
                continue;
            }

            // If working on a text element, delete part of the text object
            if (item.IsText())
            {
                var txtElement = (MsbtTextElement)item;
                string str = txtElement.Text;

                int delCount = Math.Min(str.Length - start, delLength - delPos);
                txtElement.Text = txtElement.Text.Remove(start, delCount);

                start = 0;
                elementIdx++;
                delPos += delCount;
                continue;
            }

            // If the past two conditions failed, delete the current element
            // Only occurs when a selection is one wide over a tag
            Remove(item);
            break;
        }

        // Run cleanup to merge neighbor text elements
        Cleanup();
    }

    // ====================================================== //
    // ================ Calculation Utilities =============== //
    // ====================================================== //

    public int CalcElementIdxAtCharPos(ref int charPos)
    {
        if (Count <= 1) return 0;

        int elementIdx = 0;

        // Find the starting element and align start to be local to that element
        foreach (var item in this)
        {
            // Get the char length of the current element in the page
            int itemLength = 1;
            if (item.IsText())
                itemLength = item.GetText().Length;

            // If the charPos is exactly at the end of the page, return element here
            if (charPos == itemLength && item == this.Last()) return elementIdx;

            // If the current starting position is higher than the length of this item, continue to next element
            if (charPos >= itemLength)
            {
                charPos -= itemLength;
                elementIdx++;
                continue;
            }

            return elementIdx;
        }

        throw new ArgumentOutOfRangeException(nameof(charPos));
    }
    public int CalcCharPosForElement(MsbtBaseElement element)
    {
        if (Count == 1) return 0;

        int len = 0;

        foreach (var item in this)
        {
            // Get the char length of the current element in the page
            int itemLength = 1;
            if (item.IsText())
                itemLength = item.GetText().Length;

            len += itemLength;

            if (item == element) return len;
        }

        throw new Exception("Element passed into function does not belong to this page!");
    }
    public int CalcCharLength()
    {
        int len = 0;

        foreach (var item in this)
        {
            // Get the char length of the current element in the page
            int itemLength = 1;
            if (item.IsText())
                itemLength = item.GetText().Length;

            len += itemLength;
        }

        return len;
    }
}