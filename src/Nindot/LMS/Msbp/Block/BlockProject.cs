using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbp;

public class BlockProject(byte[] data, string listingName, int offset, MsbpFile parent) : Block(data, listingName, offset, parent)
{
    private List<string> Content = [];

    protected override void InitBlock(byte[] data)
    {
        // Read how many mstxt keys are in project
        uint nameCount = BitConverter.ToUInt32(data, 0);

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
            Content.Add(Encoding.UTF8.GetString(data[offset..endPointer]));
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
        stream.Write((uint)Content.Count);

        uint offset = (uint)(0x4 + (Content.Count * 0x4));
        foreach (var s in Content)
        {
            stream.Write(offset);
            offset += (uint)(s.Length + 0x1); // Null terminator included
        }

        foreach (var s in Content)
        {
            stream.Write(Encoding.UTF8.GetBytes(s));
            stream.Write([0x00]); // Null terminator
        }
    }

    public int GetSize() { return Content.Count; }
    public ReadOnlyCollection<string> GetContent() { return new ReadOnlyCollection<string>(Content); }
    public string GetElement(int idx)
    {
        if (idx >= Content.Count)
            return null;

        return Content[idx];
    }

    internal void AddElement(string element)
    {
        if (IsBlockHeaderOK)
            Content.Add(element);
    }
    internal void RemoveElement(string element) {
        if (IsBlockHeaderOK)
            Content.Remove(element);
    }
}