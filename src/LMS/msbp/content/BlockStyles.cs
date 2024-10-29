using System;
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbp;

public class BlockStyles : Block
{
    public struct Style
    {
        public const int STYLE_BYTE_SIZE = 0x10;

        public uint RegionWidth;
        public uint LineNumber;
        public uint FontIndex;
        public uint DefaultColorIndex;

        public Style(byte[] entryData)
        {
            RegionWidth = BitConverter.ToUInt32(entryData, 0x0);
            LineNumber = BitConverter.ToUInt32(entryData, 0x4);
            FontIndex = BitConverter.ToUInt32(entryData, 0x8);
            DefaultColorIndex = BitConverter.ToUInt32(entryData, 0xC);
        }

        public void Write(ref MemoryStream stream)
        {
            stream.Write(RegionWidth);
            stream.Write(LineNumber);
            stream.Write(FontIndex);
            stream.Write(DefaultColorIndex);
        }
    }
    
    private List<Style> Styles = [];

    public BlockStyles(byte[] data, string typeName, int offset) : base(data, typeName, offset)
    {
    }

    protected override void InitBlock(byte[] data)
    {
        uint styleCount = BitConverter.ToUInt32(data, 0x0);

        for(int i = 0; i < styleCount; i++)
        {
            int pointer = (i * Style.STYLE_BYTE_SIZE) + 0x4;
            Style s = new Style(data[pointer .. (pointer + Style.STYLE_BYTE_SIZE)]);
            Styles.Add(s);
        }
    }
    
    protected override uint CalcDataSize()
    {
        return (uint)(Styles.Count * Style.STYLE_BYTE_SIZE) + sizeof(uint);
    }

    protected override void WriteBlockData(ref MemoryStream stream)
    {
        throw new System.NotImplementedException();
    }
}