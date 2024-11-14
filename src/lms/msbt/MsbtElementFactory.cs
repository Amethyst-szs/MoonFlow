using System.Collections.Generic;

namespace Nindot.LMS.Msbt.TagLib;

public class MsbtElementFactory
{
    internal virtual List<MsbtBaseElement> Build(byte[] buffer)
    {
        MsbtTextElement txt = new(buffer);
        if (txt.IsEmpty())
            return [];

        List<MsbtBaseElement> list = [txt];
        return list;
    }

    public virtual string GetFactoryName() { return "Base"; }
}