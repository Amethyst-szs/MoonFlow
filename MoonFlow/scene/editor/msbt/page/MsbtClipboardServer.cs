using System.Collections.Generic;
using System;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;
using System.Linq;
using Godot;

namespace MoonFlow.Scene.EditorMsbt;

public static class MsbtClipboardServer
{
    private static List<MsbtBaseElement> Clipboard = [];
    private static string ClipboardSystem = null;

    [StartupTask]
    public static void InitClipboardServer()
    {
        ClipboardSystem = DisplayServer.ClipboardGet();
    }

    public static void Copy(MsbtPage page, int start, int end)
    {
        // Empty current contents of clipboard
        Clipboard = [];

        int copyLength = end - start;
        int elementIdx = page.CalcElementIdxAtCharPos(ref start);
        if (elementIdx >= page.Count) return;

        // Copy data from page to clipboard
        int copyPos = 0;
        while (copyPos < copyLength)
        {
            if (elementIdx >= page.Count) throw new IndexOutOfRangeException();
            var curElement = page[elementIdx];

            // If this element is a text element, create a substring and insert into clipboard
            if (curElement.IsText())
            {
                var txtElement = (MsbtTextElement)curElement;
                string str = txtElement.GetText();

                int strCopyEndPos = Math.Min(str.Length, copyLength - copyPos + start);
                string substr = txtElement.GetText()[start..strCopyEndPos];

                Clipboard.Add(new MsbtTextElement(substr));

                copyPos += substr.Length;
                start = 0;
                elementIdx++;
                continue;
            }

            // If the current element isn't a text element, advance copy pos by 1 and insert clone
            Clipboard.Add(curElement.Clone());
            copyPos += 1;
            elementIdx++;
        }

        // Push a tagless version of the text to the system clipboard
        string taglessText = "";
        foreach (var item in Clipboard)
        {
            if (item.IsText())
                taglessText += item.GetText();
        }

        DisplayServer.ClipboardSet(taglessText);

        // Copy information about the system clipboard to determine which clipboard to use when pasting later
        ClipboardSystem = taglessText;

        return;
    }

    public static void Paste(MsbtPage page, int charIdx)
    {
        List<MsbtBaseElement> pasteContent = [];

        // Access system clipboard to determine which clipboard to use
        string sysClipboard = GetSystemClipboardWithoutCarriage();

        if (Clipboard.Count == 0 || ClipboardSystem != sysClipboard)
        {
            // Create a single text element when using system clipboard
            pasteContent.Add(new MsbtTextElement(sysClipboard));
        }
        else
        {
            // Use server's internal element clipboard when using internal clipboard
            pasteContent = Clipboard.Copy();
        }

        // Get the element index targetted by the charIdx
        int localPosition = charIdx;
        int elementIdx = page.CalcElementIdxAtCharPos(ref localPosition);

        // If the targetted element is a text element, break in half and insert in middle
        MsbtBaseElement curElement = null;
        if (elementIdx < page.Count) curElement = page[elementIdx];

        if (curElement != null && curElement.IsText())
        {
            // Split text element into a front and back half, before removing the element
            var txtElement = (MsbtTextElement)curElement;
            string str = txtElement.GetText();

            string front = str[0..localPosition];
            string back = str[localPosition..];

            page.Remove(txtElement);

            // If the front half isn't empty, reinsert into page
            if (front.Length > 0)
            {
                page.Insert(elementIdx, new MsbtTextElement(front));
                elementIdx++;
            }

            // Insert all data from clipboard
            foreach (var item in pasteContent)
            {
                page.Insert(elementIdx, item.Clone());
                elementIdx++;
            }

            // If the back half isn't empty, reinsert into page
            if (back.Length > 0)
                page.Insert(elementIdx, new MsbtTextElement(back));

            // Finally, run the cleanup routine to merge neighbor text elements
            page.Cleanup();
            return;
        }

        // If the element isn't a text element, insert everything in clipboard at current position and cleanup
        if (curElement == page.Last() && localPosition > 0)
            elementIdx++;

        foreach (var item in pasteContent)
        {
            page.Insert(elementIdx, item.Clone());
            elementIdx++;
        }

        page.Cleanup();
    }

    public static List<MsbtBaseElement> GetClipboard()
    {
        return Clipboard;
    }

    public static string GetClipboardAsString()
    {
        // Access system clipboard to determine which clipboard to use
        string sysClipboard = GetSystemClipboardWithoutCarriage();
        if (Clipboard.Count == 0 || ClipboardSystem != sysClipboard)
            return sysClipboard;

        // Otherwise use internal clipboard server which can contain tag elements
        string str = "";

        foreach (var item in Clipboard)
        {
            if (item.IsText())
                str += item.GetText();
            else
                str += '\u2E3A';
        }

        return str;
    }

    private static string GetSystemClipboardWithoutCarriage()
    {
        return DisplayServer.ClipboardGet().Replace("\r", "");
    }
}
