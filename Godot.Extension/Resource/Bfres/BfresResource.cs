using System;
using System.IO;
using System.Collections.Generic;
using Godot;

using BfresLibrary;
using Nindot;
using System.Linq;

namespace Godot.Extension.Resources;

public partial class BfresResource : ResFile
{
    public BfresResource(string path) : base(path) { }
    internal BfresResource(Stream stream) : base(stream) { }

    public static BfresResource FromSarcFile(SarcFile sarc)
    {
        string lookup = sarc.Name.Replace(".szs", ".bfres");
        if (!lookup.EndsWith(".bfres")) lookup += ".bfres";

        if (sarc.Content.ContainsKey(lookup))
            return FromSarcFile(sarc, lookup);

        lookup = sarc.Content.Keys.First(f => f.EndsWith(".bfres"));
        if (lookup != null)
            return FromSarcFile(sarc, lookup);
        
        return null;
    }
    public static BfresResource FromSarcFile(SarcFile sarc, string name)
    {
        if (!sarc.Content.TryGetValue(name, out ArraySegment<byte> data))
            throw new KeyNotFoundException("Could not find " + name);

        var stream = new MemoryStream([.. data]);
        return new BfresResource(stream);
    }
}