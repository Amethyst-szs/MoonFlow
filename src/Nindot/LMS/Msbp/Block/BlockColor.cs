using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbp;

public class BlockColor(byte[] data, string typeName, int offset, MsbpFile parent) : Block(data, typeName, offset, parent)
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

        public void WriteColor(MemoryStream stream)
        {
            stream.Write(R);
            stream.Write(G);
            stream.Write(B);
            stream.Write(A);
        }
        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Entry))
                return base.Equals(obj);
            
            var cmp = (Entry)obj;
            return R == cmp.R && G == cmp.G && B == cmp.B && A == cmp.A;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    private List<Entry> Colors = [];

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

    protected override void WriteBlockData(MemoryStream stream)
    {
        stream.Write((uint)Colors.Count);

        foreach (var c in Colors)
        {
            c.WriteColor(stream);
        }
    }

    public ReadOnlyCollection<Entry> GetColorList()
    {
        return new ReadOnlyCollection<Entry>(Colors);
    }

    public Entry GetColor(int idx)
    {
        if (idx >= Colors.Count)
            return null;

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