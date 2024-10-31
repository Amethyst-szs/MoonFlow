using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbp;

public class BlockAttributeLists : Block
{
    List<List<string>> Lists = [];

    public BlockAttributeLists(byte[] data, string listingName, int offset) : base(data, listingName, offset)
    {
    }

    protected override void InitBlock(byte[] data)
    {
        // Read how many attribute lists are in block
        uint listCount = BitConverter.ToUInt32(data, 0);

        // Iterate over all lists
        for (int i = 0; i < listCount; i++)
        {
            // Create the current list
            int offset = (int)BitConverter.ToUInt32(data, (i * 4) + 4);
            InitList(data, offset);
        }

        return;
    }

    private void InitList(byte[] data, int offset)
    {
        List<string> l = [];

        uint nameCount = BitConverter.ToUInt32(data, offset);
        offset += sizeof(uint);

        // The code in this loop is really bad but this particular block is really finicky, just
        // trust that it works and pretend it isn't awful :)
        for (int i = 0; i < nameCount; i++)
        {
            // Create the current list
            int itemPointer = (int)BitConverter.ToUInt32(data, offset + (i * 4));
            itemPointer += 8;

            int endPointer;
            if (i < nameCount - 1)
            {
                endPointer = (int)BitConverter.ToUInt32(data, offset + ((i + 1) * 4)) + 7;
            }
            else
            {
                endPointer = data.Length - 1;
                while (data[endPointer] == 0x00)
                {
                    endPointer--;
                }
                endPointer++;
            }

            // Create array segment and append attribute name to list
            l.Add(data[itemPointer..endPointer].GetStringFromUtf8());
        }

        Lists.Add(l);
    }

    protected override uint CalcDataSize()
    {
        uint size = 0x4; // List count

        foreach (var l in Lists)
        {
            // Every list's offset value takes 4 bytes, and the size of the list takes 4 bytes
            size += 0x8;

            foreach (var s in l)
            {
                size += (uint)(0x4 + s.Length + 0x1);
            }
        }

        return size;
    }

    // This code is ass and might not even work correctly, oops!
    protected override void WriteBlockData(MemoryStream stream)
    {
        GD.PushWarning("Writing attribute list blocks is currently experimental! The output MSBP may not be functional!");

        stream.Write((uint)Lists.Count);

        // This first iteration will write down all the of attr list offsets
        uint offset = (uint)(0x4 + (Lists.Count * 4));
        foreach (var nameList in Lists)
        {
            stream.Write(offset);

            foreach (var name in nameList)
            {
                offset += (uint)name.Length;
            }
        }

        // This second iteration will write the actual lists and their strings
        offset = 0;
        foreach (var nameList in Lists)
        {
            stream.Write((uint)nameList.Count);

            foreach (var name in nameList)
            {
                stream.Write((uint)((nameList.Count * 4) + offset + 4));
                offset += (uint)(name.Length + 1);
            }
            
            foreach (var name in nameList)
            {
                stream.Write(name.ToUtf8Buffer());
                stream.Write([0x00]); // Null Terminator
            }
        }
    }

    public ReadOnlyCollection<string> GetList(int idx)
    {
        if (idx >= Lists.Count)
            return new ReadOnlyCollection<string>([]);

        return new ReadOnlyCollection<string>(Lists[idx]);
    }
}