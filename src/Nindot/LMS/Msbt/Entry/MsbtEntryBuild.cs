using System.IO;
using System.Linq;

using CommunityToolkit.HighPerformance;

using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace Nindot.LMS.Msbt;

public partial class MsbtEntry
{
    public byte[] BuildText()
    {
        // If the element list is empty, return a two-byte null terminator and exit
        if (Pages.Count == 0)
            return [0x00, 0x00];

        // Clean data before packing together a memory stream
        Cleanup();

        // Create an instance of a page break tag to use between each page
        var pageBreak = new MsbtTagElementSystemPageBreak();

        // Create a stream and write each page to it
        MemoryStream stream = new();
        foreach (var page in Pages)
        {
            page.WriteBytes(stream);

            // If this page isn't the last page, add a page break tag
            if (Pages.Last() != page)
                pageBreak.WriteBytes(stream);
        }

        // Append a two-byte null terminator to entry
        stream.Write((ushort)0);
        return stream.ToArray();
    }

    public void Cleanup()
    {
        foreach (var page in Pages) page.Cleanup();
    }
}