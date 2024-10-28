using System.IO;

namespace Nindot.LMS.Msbp;

class BlockColor : Block
{
    public BlockColor(byte[] data, string typeName) : base(data, typeName)
    {
    }

    protected override uint CalcDataSize()
    {
        throw new System.NotImplementedException();
    }

    protected override void InitBlock(byte[] data)
    {
        throw new System.NotImplementedException();
    }

    protected override void WriteBlockData(ref MemoryStream stream)
    {
        throw new System.NotImplementedException();
    }
}