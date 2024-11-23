using System.Collections.Generic;

namespace Nindot.LMS.Msbt.TagLib;

public class MsbtElementFactory
{
    internal virtual List<MsbtPage> Build(byte[] buffer)
    {
        MsbtPage page = [];

        MsbtTextElement txt = new(buffer);
        if (txt.IsEmpty())
            return [];

        return [page];
    }

    public virtual string GetFactoryName() { return "Base"; }
}