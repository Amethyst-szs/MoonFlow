using System;
using System.IO;
using System.Collections.Generic;
using Godot;

using CommunityToolkit.HighPerformance;
using System.Linq;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementProjectIcon : MsbtTagElement
{
    private List<string> _iconTable = [];
    private readonly bool _isInvalid = false;

    public MsbtTagElementProjectIcon(ref int pointer, byte[] buffer, MsbtFile parent) : base(ref pointer, buffer, parent)
    {
        // Iterate through the buffer, copying strings until DataSize is maxed out
        int progress = 0;

        while (progress < DataSize)
        {
            ushort strLen = BitConverter.ToUInt16(buffer, pointer);
            progress += 2;
            pointer += 2;

            byte[] strBuf = buffer[pointer..(pointer + strLen)];
            _iconTable.Add(strBuf.GetStringFromUtf16());

            progress += strLen;
            pointer += strLen;
        }

        // If progress has overshot the DataSize at all, some piece of data is corrupted!
        // If corrupted, setup a default project icon state along with setting the IsInvalid flag
        if (progress > DataSize)
        {
            _iconTable = [];
            DataSize = CalcDataSize();

            _isInvalid = true;
            return;
        }

        // Ensure DataSize property is up-to-date after parsing strings
        DataSize = CalcDataSize();
        return;
    }

    public ushort CalcDataSize()
    {
        ushort value = 0;

        foreach (string icon in _iconTable)
        {
            ushort strLen = (ushort)icon.ToUtf16Buffer().Length;
            value += (ushort)(strLen + 2);
        }

        return value;
    }

    public void UpdateIconTableByTag()
    {
        string tagLabel = GetTagNameStr();

        _iconTable.Clear();
        foreach (var prefix in _iconTablePrefixes)
        {
            _iconTable.Add(tagLabel.Replace("PadStyle", prefix));
        }
    }

    public void SetIcon(ushort newTagIndex)
    {
        TagName = newTagIndex;
        UpdateIconTableByTag();
    }

    public void SetIcon(string padStyleLabel)
    {
        Msbp.TagGroupInfo group = Parent.Project.TagGroupGet(GroupName);
        if (group == null)
            return;
        
        int tagIdx = Parent.Project.TagGetIndex(padStyleLabel);
        if (tagIdx == -1)
            return;

        TagName = (ushort)tagIdx;
        UpdateIconTableByTag();
    }

    public override byte[] GetBytes()
    {
        // Ensure DataSize is up-to-date with table strings
        DataSize = CalcDataSize();

        // Now that data size is correct, stream can be created
        MemoryStream value = CreateMemoryStreamWithHeaderData();

        foreach (string icon in _iconTable)
        {
            byte[] buf = icon.ToUtf16Buffer();
            ushort strLen = (ushort)buf.Length;

            value.Write(strLen);
            value.Write(buf);
        }

        return value.ToArray();
    }

    public override bool IsValid()
    {
        return !_isInvalid;
    }

    public override bool IsFixedDataSize()
    {
        return false;
    }

    public override string GetTagNameStr()
    {
        Msbp.TagGroupInfo group = Parent.Project.TagGroupGet(GroupName);
        if (group == null)
            return "Unknown";
        
        int tagIdx = group.ListingIndexList[TagName];
        Msbp.TagInfo tag = Parent.Project.TagGet(tagIdx);
        if (tag == null)
            return "Unknown";
        
        return tag.Name;
    }

    private static readonly string[] _iconTablePrefixes = [
        "Dual",
        "Handheld",
        "FullKey",
        "Left",
        "Right",
    ];
};