using System.IO;
using System.Collections.Generic;

using Nindot.LMS.Msbt.TagLib;

namespace Nindot.LMS.Msbt;

public partial class MsbtEntry
{
    public string Name { get; internal set; } = "";
    internal MsbtElementFactory Factory = null;

    public List<MsbtPage> Pages { get; private set; } = [];
    internal uint StyleIndex = 0xFFFFFFFF;

    internal MsbtEntry(MsbtElementFactory factory, string name, byte[] txtData, uint styleIndex = 0xFFFFFFFF)
    {
        Factory = factory;
        Name = name;
        Pages = factory.Build(txtData);
        StyleIndex = styleIndex;
    }
    public MsbtEntry(MsbtElementFactory factory, string name)
    {
        Factory = factory;
        Name = name;
        Pages.Add(new MsbtPage(new MsbtTextElement("")));
    }
    public MsbtEntry(MsbtElementFactory factory, string name, string textContent)
    {
        Factory = factory;
        Name = name;
        Pages.Add(new MsbtPage(new MsbtTextElement(textContent)));
    }
}