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

    public MsbtTagElementProjectIcon(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
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

    public void SetIcon(ushort tagIdx)
    {
        TagName = tagIdx;
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
        return "Project Icon " + TagName;
    }

    private static readonly string[] _iconTablePrefixes = [
        "Dual",
        "Handheld",
        "FullKey",
        "Left",
        "Right",
    ];
};