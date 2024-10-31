using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.HighPerformance;
using Godot;

namespace Nindot.LMS.Msbt;

public class BlockAttributeData : Block
{
    // ##################################################################### //
    // # This class is currently a placeholder! A proper implementation of # //
    // ########### attribute data in msbts is not functional yet! ########## //
    // ##################################################################### //
    
    private byte[] _attributeData = [];

    public BlockAttributeData(byte[] data, string name, int offset) : base(data, name, offset)
    {
    }

    public BlockAttributeData(List<object> list, string name) : base(list, name)
    {
    }

    protected override void InitBlock(byte[] data)
    {
        _attributeData = data;
    }

    protected override void InitBlockWithList(List<object> list)
    {
        if (list.GetType() != typeof(List<byte[]>) || list.Count > 1)
        {
            GD.PushError("Invalid list type in BlockStyleIndex - InitBlockWithList!");
            return;
        }

        _attributeData = list.Cast<byte[]>().ToList()[0];
    }

    protected override uint CalcDataSize()
    {
        return (uint)_attributeData.Length;
    }

    protected override void WriteBlockData(MemoryStream stream)
    {
        stream.Write(_attributeData);
    }
}