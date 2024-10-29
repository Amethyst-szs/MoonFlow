using System;
using System.Collections.Generic;
using System.IO;
using Godot;

namespace Nindot.LMS.Msbp;

public class BlockTagParams : Block
{
    public class ParamInfo
    {
        public byte ParamType;
        public string Name;

        public ParamInfo()
        {
        }

        public ParamInfo(byte[] paramData)
        {
            ParamType = paramData[0];
            Name = paramData[1 .. (paramData.Length - 1)].GetStringFromUtf8();
        }

        public virtual int CalcSizeBytes()
        {
            //     ParamType      Length of name plus null terminator
            return sizeof(byte) + Name.Length + sizeof(byte);
        }

        public virtual void WriteLabel(ref MemoryStream stream)
        {
        }
    }

    public class ParamInfoArray : ParamInfo
    {
        public ushort ItemCount;
        public List<ushort> ItemIndexList = [];

        public ParamInfoArray(byte[] paramData) : base()
        {
            ParamType = paramData[0];

            int pointer = 2;
            ItemCount = BitConverter.ToUInt16(paramData, pointer);
            pointer += 2;

            // Read all listing indexes in the table used to access another block's keys
            while (pointer < (ItemCount * 2) + sizeof(ushort))
            {
                ItemIndexList.Add(BitConverter.ToUInt16(paramData, pointer));
                pointer += 2;
            }

            Name = paramData[pointer .. (paramData.Length - 1)].GetStringFromUtf8();
        }

        public override int CalcSizeBytes()
        {
            //     ListingCount     Byte size of ListingIndexList               Length of name plus null terminator
            return sizeof(ushort) + (ItemIndexList.Count * sizeof(ushort)) + Name.Length + sizeof(byte);
        }

        public override void WriteLabel(ref MemoryStream stream)
        {
        }
    }

    List<ParamInfo> ParamList = [];

    public BlockTagParams(byte[] data, string listingName, int offset) : base(data, listingName, offset)
    {
    }

    protected override void InitBlock(byte[] data)
    {
        // Read how many params are in tag
        ushort paramCount = BitConverter.ToUInt16(data, 0);

        // Iterate over all param offset entries
        for (int i = 0; i < paramCount; i++)
        {
            // Get the offset for the current param entry in block
            int offset = (int)BitConverter.ToUInt32(data, (i * 4) + 4);

            // Calculate the end offset of the param info segment
            int endOffset = offset + sizeof(byte);

            // If the current param type is 0x9, extra data will need to be read because that param type is for an array
            byte paramType = data[offset];
            if (paramType == 0x9)
            {
                ushort entryCount = BitConverter.ToUInt16(data, offset + 0x2);
                endOffset += entryCount * 2;
            }
            
            // Regardless of param type, now append the length of the string to the end offset
            while (endOffset < data.Length)
            {
                endOffset++;

                if (data[endOffset - 1] == 0x00)
                    break;
            }

            // Create array segment and generate group data
            byte[] segment = data[offset..endOffset];

            if (paramType == 0x9)
                ParamList.Add(new ParamInfoArray(segment));
            else
                ParamList.Add(new ParamInfo(segment));
        }

        return;
    }

    protected override uint CalcDataSize()
    {
        uint size = 0x4; // Tag param count and padding

        foreach (var p in ParamList)
        {
            size += (uint)(0x4 + p.CalcSizeBytes());
        }

        return size;
    }

    protected override void WriteBlockData(ref MemoryStream stream)
    {
        throw new NotImplementedException();
    }
}