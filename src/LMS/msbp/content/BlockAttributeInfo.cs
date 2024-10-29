using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbp;

public class BlockAttributeInfo : Block
{
    public class Entry
    {
        public const int ATTRIBUTE_BYTE_SIZE = 0x8;

        public byte Type;
        public ushort ListIndex; // Only used if type is 9 to access ALI2 (Attribute Lists)
        public uint Offset;

        public Entry(byte[] entryData)
        {
            Type = entryData[0];
            ListIndex = BitConverter.ToUInt16(entryData, 2);
            Offset = BitConverter.ToUInt32(entryData, 4);
        }

        public void Write(ref MemoryStream stream)
        {
            stream.Write(Type);
            stream.Write(ListIndex);
            stream.Write(Offset);
        }
    }
    
    private List<Entry> Attributes = [];

    public BlockAttributeInfo(byte[] data, string typeName, int offset) : base(data, typeName, offset)
    {
    }

    protected override void InitBlock(byte[] data)
    {
        uint attrCount = BitConverter.ToUInt32(data, 0x0);

        for(int i = 0; i < attrCount; i++)
        {
            int pointer = (i * Entry.ATTRIBUTE_BYTE_SIZE) + 0x4;
            Entry e = new Entry(data[pointer .. (pointer + Entry.ATTRIBUTE_BYTE_SIZE)]);
            Attributes.Add(e);
        }
    }
    
    protected override uint CalcDataSize()
    {
        return (uint)(Attributes.Count * Entry.ATTRIBUTE_BYTE_SIZE) + sizeof(uint);
    }

    protected override void WriteBlockData(ref MemoryStream stream)
    {
        throw new System.NotImplementedException();
    }

    public ReadOnlyCollection<Entry> GetInfoList()
    {
        return new ReadOnlyCollection<Entry>(Attributes);
    }

    public Entry GetAttribute(int idx)
    {
        if (idx >= Attributes.Count)
            return null;
        
        return Attributes[idx];
    }
}