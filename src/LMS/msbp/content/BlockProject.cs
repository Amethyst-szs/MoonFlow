using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Godot;

namespace Nindot.LMS.Msbp;

public class BlockProject : Block
{
    List<string> Content = [];

    public BlockProject(byte[] data, string listingName, int offset) : base(data, listingName, offset) { }

    protected override void InitBlock(byte[] data)
    {
        // Read how many mstxt keys are in project
        ushort nameCount = BitConverter.ToUInt16(data, 0);

        // Iterate over every single key (oh boy we in for a long one)
        for (int i = 0; i < nameCount; i++)
        {
            // Get the offset for the current name in block
            int offset = (int)BitConverter.ToUInt32(data, (i * 4) + 4);

            // Calculate the end offset of the name segment
            int endPointer;
            if (i < nameCount - 1)
            {
                endPointer = (int)BitConverter.ToUInt32(data, ((i + 1) * 4) + 4) - 1;
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

            // Create array segment and append name to list
            Content.Add(data[offset..endPointer].GetStringFromUtf8());
        }

        return;
    }

    protected override uint CalcDataSize()
    {
        uint size = 0x4; // Content count

        foreach (var p in Content)
        {
            // Offset + String Length + Null Terminator
            size += (uint)(0x4 + p.Length + 0x1);
        }

        return size;
    }

    protected override void WriteBlockData(MemoryStream stream)
    {
        throw new NotImplementedException();
    }

    public int GetSize() { return Content.Count; }
    public ReadOnlyCollection<string> GetContent() { return new ReadOnlyCollection<string>(Content); }
    public string GetElement(int idx)
    {
        if (idx >= Content.Count)
            return null;

        return Content[idx];
    }

    internal void AddElement(string element) { Content.Add(element); }
    internal void RemoveElement(string element) { Content.Remove(element); }
}