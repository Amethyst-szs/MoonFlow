using System.IO;

namespace Nindot.LMS.Msbt;

public class BlockAttributeData(byte[] data, string name, int offset, MsbtFile parent) : Block(data, name, offset, parent)
{
    // ##################################################################### //
    // # This class is currently a placeholder! A proper implementation of # //
    // ########### attribute data in msbts is not functional yet! ########## //
    // ##################################################################### //

    private byte[] _attributeData = [];

    protected override void InitBlock(byte[] data)
    {
        _attributeData = data;
    }

    protected override uint CalcDataSize()
    {
        return (uint)_attributeData.Length;
    }

    protected override void WriteBlockData(MemoryStream stream)
    {
        stream.Write(_attributeData);
    }
}