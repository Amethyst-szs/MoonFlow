using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbp;

public class BlockTagParams(byte[] data, string listingName, int offset, MsbpFile parent) : Block(data, listingName, offset, parent)
{
    List<TagParamInfo> ParamList = [];

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
            if (paramType == TagParamInfoTypeArray.TYPE_ID_ARRAY)
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

            if (paramType == TagParamInfoTypeArray.TYPE_ID_ARRAY)
                ParamList.Add(new TagParamInfoTypeArray(segment));
            else
                ParamList.Add(new TagParamInfo(segment));
        }

        return;
    }

    protected override uint CalcDataSize()
    {
        uint size = 0x4; // Tag param count and padding

        foreach (var p in ParamList)
        {
            size += (uint)(0x4 + p.CalcSizeBytes((int)size));
        }

        return size;
    }

    protected override void WriteBlockData(MemoryStream stream)
    {
        stream.Write((ushort)ParamList.Count);
        stream.Write((ushort)0x0000); // Padding

        // Create offset table before the group info
        int offset = 0x4 + (ParamList.Count * sizeof(uint));

        foreach (var item in ParamList)
        {
            stream.Write((uint)offset);
            offset += item.CalcSizeBytes(offset);
        }

        // Now actually write each group's information
        foreach (var item in ParamList)
        {
            item.Write(stream);
        }
    }

    public int GetParamCount(TagInfo tag)
    {
        if (!tag.IsTag())
            return -1;

        return tag.ListingIndexList.Count;
    }

    public TagParamInfo GetParam(int idx)
    {
        if (idx >= ParamList.Count)
            return null;

        return ParamList[idx];
    }

    internal ReadOnlyCollection<TagParamInfo> GetParamsForTag(TagInfo tag)
    {
        if (!tag.IsTag())
            return new ReadOnlyCollection<TagParamInfo>([]);

        int paramCount = tag.ListingIndexList.Count;
        TagParamInfo[] paramList = new TagParamInfo[paramCount];

        for (int idx = 0; idx < paramCount; idx++)
        {
            paramList[idx] = ParamList[tag.ListingIndexList[idx]];
        }

        return new ReadOnlyCollection<TagParamInfo>(paramList);
    }
}