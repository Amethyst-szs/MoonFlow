using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Nindot.LMS.Msbp;

class BlockColor : Block
{
    public struct Entry
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;
    }
    
    private List<Entry> Colors = [];

    public BlockColor(byte[] data, string typeName, int offset) : base(data, typeName, offset)
    {
    }

    protected override void InitBlock(byte[] data)
    {
        uint colorCount = BitConverter.ToUInt32(data, 0x0);
        
        int pointer = sizeof(uint);

        for(uint i = 0; i < colorCount; i++)
        {
            Entry c = new();
            c.R = data[pointer + 0];
            c.G = data[pointer + 1];
            c.B = data[pointer + 2];
            c.A = data[pointer + 3];

            Colors.Add(c);

            pointer += 0x4;
        }
    }
    
    protected override uint CalcDataSize()
    {
        throw new System.NotImplementedException();
    }

    protected override void WriteBlockData(ref MemoryStream stream)
    {
        throw new System.NotImplementedException();
    }
}