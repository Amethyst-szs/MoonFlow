using System.IO;

using CommunityToolkit.HighPerformance;

using Nindot.LMS.Msbt.TagLib;

namespace Nindot.LMS.Msbt;

public partial class MsbtEntry
{
    public byte[] BuildText()
    {
        // If the element list is empty, return a two-byte null terminator and exit
        if (Elements.Count == 0)
            return [0x00, 0x00];

        // Clean element list before packing together a memory stream
        CleanupElementList();

        // Create a stream and write each element to it
        MemoryStream stream = new();
        foreach (var item in Elements)
        {
            item.WriteBytes(stream);
        }

        // Append a two-byte null terminator to entry
        stream.Write((ushort)0);
        return stream.ToArray();
    }

    private void CleanupElementList()
    {
        // Iterate through all elements
        int elementIdx = 0;
        while (elementIdx < Elements.Count - 1)
        {
            // Get the current element and skip forward if it isn't a text element
            MsbtBaseElement cur = Elements[elementIdx];
            if (cur.GetType() != typeof(MsbtTextElement))
            {
                elementIdx++;
                continue;
            }

            MsbtTextElement curT = (MsbtTextElement)cur;

            // Attempt to merge neighboring text elements if they exist
            MsbtBaseElement next = Elements[elementIdx + 1];
            if (next.GetType() == typeof(MsbtTextElement))
            {
                MsbtTextElement nextT = (MsbtTextElement)next;
                curT.Text += nextT.Text;

                Elements.Remove(nextT);
            }

            // Remove any null terminators from curT
            curT.RemoveNullTerminator();

            // Advance to next element
            elementIdx++;
        }
    }
}