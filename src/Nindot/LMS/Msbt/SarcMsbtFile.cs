using Nindot.LMS.Msbt.TagLib;

namespace Nindot.LMS.Msbt;

public class SarcMsbtFile(MsbtElementFactory factory, byte[] data, string name, SarcFile sarc)
    : MsbtFile(factory, data, name)
{
    public SarcFile Sarc { get; private set; } = sarc;
}