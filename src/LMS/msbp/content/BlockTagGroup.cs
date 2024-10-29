using System;
using System.Collections.Generic;
using System.IO;
using Godot;

namespace Nindot.LMS.Msbp;

public class BlockTagGroup : Block
{
    public struct Group
    {
        public ushort TagCount;
        public List<ushort> TagIndexList = [];
        public string Name;

        public Group(byte[] groupData)
        {
            int pointer = 0;

            // Read tag count from buffer
            TagCount = BitConverter.ToUInt16(groupData, pointer);
            pointer += 2;

            // Read all tag indexes in the table used to access TAG2
            while (pointer < (TagCount * 2) + sizeof(ushort))
            {
                TagIndexList.Add(BitConverter.ToUInt16(groupData, pointer));
                pointer += 2;
            }

            // Read remainder of data into the name, removing the null terminator at the end
            int strLen = groupData.Length - 1;
            Name = groupData[pointer..strLen].GetStringFromUtf8();
        }

        public int CalcSizeBytes()
        {
            //     TagCount          Byte size of TagIndexList              Length of name plus null terminator
            return sizeof(ushort) + (TagIndexList.Count * sizeof(ushort)) + Name.Length + sizeof(byte);
        }

        public void WriteLabel(ref MemoryStream stream)
        {
        }
    }

    List<Group> GroupList = [];

    public BlockTagGroup(byte[] data, string typeName, int offset) : base(data, typeName, offset)
    {
    }

    protected override void InitBlock(byte[] data)
    {
        // Read how many tag groups we in project
        ushort groupCount = BitConverter.ToUInt16(data, 0);

        // Iterate over all group offset entries
        for (int i = 0; i < groupCount; i++)
        {
            // Get the offset for the current tag group entry in block
            int offset = (int)BitConverter.ToUInt32(data, (i * 4) + 4);
            offset += 2; // I don't understand why I have to do this but the bytes don't line up with the offset otherwise

            // Get the amount of tags in the current group (will be needed to generate data segment)
            ushort tagCount = BitConverter.ToUInt16(data, offset);

            // Create the end offset, starting at the start of the group name string. This
            // will be increased until finding the null terminator. The plus sizeof(int) on
            // the end is because the offset values in the binary are relative to the end
            // of the padding bytes after the group count
            int endOffset = offset + (tagCount * 2) + sizeof(int);
            
            while (endOffset < data.Length)
            {
                endOffset++;

                if (data[endOffset - 1] == 0x00)
                    break;
            }

            // Create array segment and generate group data
            byte[] segment = data[offset..endOffset];
            GroupList.Add(new Group(segment));
            continue;
        }

        return;
    }

    protected override uint CalcDataSize()
    {
        uint size = 0x4; // Tag group count and padding

        foreach (var g in GroupList)
        {
            size += (uint)(0x4 + g.CalcSizeBytes());
        }

        return size;
    }

    protected override void WriteBlockData(ref MemoryStream stream)
    {
        throw new NotImplementedException();
    }
}