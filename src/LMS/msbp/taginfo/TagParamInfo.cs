using System;
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.HighPerformance;
using Godot;

namespace Nindot.LMS.Msbp;

public class TagParamInfo
{
    public const uint PARAM_ALIGNMENT_SIZE = 0x4;

    public byte ParamType;
    public string Name;

    public TagParamInfo() {}
    public TagParamInfo(byte[] paramData)
    {
        ParamType = paramData[0];
        Name = paramData[1..(paramData.Length - 1)].GetStringFromUtf8();
    }

    public virtual int CalcSizeBytes(int position)
    {
        // ParamType, String Length, Null Terminator
        int size = sizeof(byte) + Name.Length + 0x1;

        // The size must align onto a GROUP_ALIGNMENT_SIZE grid, use the position and size to calculate
        while ((position + size) % PARAM_ALIGNMENT_SIZE != 0)
        {
            size += 1;
        }

        return size;
    }

    public virtual void Write(MemoryStream stream)
    {
        stream.Write(ParamType);
        stream.Write(Name.ToUtf8Buffer());
        stream.Write((byte)0x00); // Null Terminator

        // Align stream to PARAM_ALIGNMENT_SIZE
        while (stream.Position % PARAM_ALIGNMENT_SIZE != 0)
        {
            stream.Write([0x00]);
        }
    }
}

public class TagParamInfoTypeArray : TagParamInfo
{
    public const uint TYPE_ID_ARRAY = 9;

    public List<ushort> ItemIndexList = [];

    public TagParamInfoTypeArray(byte[] paramData) : base()
    {
        ParamType = paramData[0];
        if (ParamType != TYPE_ID_ARRAY) {
            GD.PushError("Attempting to create a TagParamInfoTypeArray using data that isn't TYPE_ID_ARRAY!");
            return;
        }

        int pointer = 2;
        var itemCount = BitConverter.ToUInt16(paramData, pointer);
        pointer += 2;

        // Read all listing indexes in the table used to access another block's keys
        while (pointer < (itemCount * 2) + sizeof(ushort))
        {
            ItemIndexList.Add(BitConverter.ToUInt16(paramData, pointer));
            pointer += 2;
        }

        Name = paramData[pointer..(paramData.Length - 1)].GetStringFromUtf8();
    }

    public override int CalcSizeBytes(int position)
    {
        // ParamType, String Length, Null Terminator
        int size = sizeof(byte) + Name.Length + 0x1;

        // ItemCount ushort, ItemIndexList
        size += sizeof(ushort) + (ItemIndexList.Count * 2);

        // The size must align onto a GROUP_ALIGNMENT_SIZE grid, use the position and size to calculate
        while ((position + size) % PARAM_ALIGNMENT_SIZE != 0)
        {
            size += 1;
        }

        return size;
    }

    public override void Write(MemoryStream stream)
    {
        stream.Write(ParamType);
        stream.Write((ushort)ItemIndexList.Count);
        
        foreach (var item in ItemIndexList)
        {
            stream.Write(item);
        }

        stream.Write(Name.ToUtf8Buffer());
        stream.Write((byte)0x00); // Null Terminator

        // Align stream to PARAM_ALIGNMENT_SIZE
        while (stream.Position % PARAM_ALIGNMENT_SIZE != 0)
        {
            stream.Write([0x00]);
        }
    }
}