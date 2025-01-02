using System.IO;
using System.Collections.Generic;

using Nindot.LMS.Msbt.TagLib;
using System.Linq;

namespace Nindot.LMS.Msbt;

public partial class MsbtEntry
{
    public string Name { get; internal set; } = "";
    internal MsbtElementFactory Factory = null;

    public List<MsbtPage> Pages { get; private set; } = [];
    internal uint StyleIndex = 0xFFFFFFFF;

    public bool IsModified { get; private set; } = false;

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

    public MsbtEntry CloneDeep()
    {
        var clone = new MsbtEntry(Factory, Name)
        {
            StyleIndex = StyleIndex,
            IsModified = IsModified
        };

        clone.Pages.Clear();
        
        foreach (var page in Pages)
            clone.Pages.Add(page.Clone());

        return clone;
    }
    public override bool Equals(object obj)
    {
        // Convert type
        if (obj.GetType() != typeof(MsbtEntry)) return false;
        var b = (MsbtEntry)obj;

        // Compare fields
        if (Name != b.Name) return false;
        if (StyleIndex != b.StyleIndex) return false;
        if (Pages.Count != b.Pages.Count) return false;

        // Compare pages
        for (int p = 0; p < Pages.Count; p++)
        {
            var ap = Pages[p];
            var bp = b.Pages[p];

            if (ap.Count != bp.Count) return false;

            for (int e = 0; e < ap.Count; e++)
            {
                var ae = ap[e];
                var be = bp[e];

                if (!ae.GetBytes().SequenceEqual(be.GetBytes()))
                    return false;
            }
        }

        return true;
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public void SetModifiedFlag() { IsModified = true; }
    public void ResetModifiedFlag() { IsModified = false; }
}