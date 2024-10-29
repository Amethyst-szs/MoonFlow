using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Nindot.LMS.Msbp;

public class BlockColor : Block
{
    public class Entry
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Entry(byte[] data, int pointer)
        {
            R = data[pointer + 0];
            G = data[pointer + 1];
            B = data[pointer + 2];
            A = data[pointer + 3];
        }

        public Entry(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }

    private List<Entry> Colors = [];

    public BlockColor(byte[] data, string typeName, int offset) : base(data, typeName, offset)
    {
    }

    protected override void InitBlock(byte[] data)
    {
        uint colorCount = BitConverter.ToUInt32(data, 0x0);

        int pointer = sizeof(uint);

        for (uint i = 0; i < colorCount; i++)
        {
            Colors.Add(new Entry(data, pointer));
            pointer += 0x4;
        }
    }

    protected override uint CalcDataSize()
    {
        return (uint)(Colors.Count * 0x4) + sizeof(uint);
    }

    protected override void WriteBlockData(ref MemoryStream stream)
    {
        throw new System.NotImplementedException();
    }

    public ReadOnlyCollection<Entry> GetColorList()
    {
        return new ReadOnlyCollection<Entry>(Colors);
    }

    public Entry GetColor(int idx)
    {
        return Colors[idx];
    }

    internal int AddColor(Entry c)
    {
        Colors.Add(c);
        return Colors.IndexOf(c);
    }

    internal void MoveColor(int startIndex, int endIndex)
    {
        // Ensure start and end index are both within the bounds of the list
        if (startIndex < 0 || startIndex >= Colors.Count || endIndex < 0 || endIndex >= Colors.Count)
            return;
        
        Entry c = Colors[startIndex];
        Colors.Remove(c);
        Colors.Insert(endIndex, c);
    }

    internal void RemoveColor(int idx)
    {
        if (idx >= Colors.Count)
            return;
        
        Colors.RemoveAt(idx);
    }
}