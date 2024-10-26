using System;
using System.IO;
using System.Collections.Generic;
using Godot;

using CommunityToolkit.HighPerformance;
using System.Linq;

namespace Nindot.MsbtTagLibrary.Smo;

public class MsbtTagElementProjectIcon : MsbtTagElement
{
    public List<string> IconTable = [];

    private bool IsInvalid = false;

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
            IconTable.Add(strBuf.GetStringFromUtf16());

            progress += strLen;
            pointer += strLen;
        }

        // If progress has overshot the DataSize at all, some piece of data is corrupted!
        // If corrupted, setup a default project icon state along with setting the IsInvalid flag
        if (progress > DataSize) {
            IconTable = [];
            DataSize = CalcDataSize();

            IsInvalid = true;
            return;
        }

        // Ensure DataSize property is up-to-date after parsing strings
        DataSize = CalcDataSize();
        return;
    }

    public ushort CalcDataSize()
    {
        ushort value = 0;

        foreach (string icon in IconTable)
        {
            ushort strLen = (ushort)icon.ToUtf16Buffer().Length;
            value += (ushort)(strLen + 2);
        }

        return value;
    }

    public bool TryGetIconType(out TagNameProjectIcon name)
    {
        if (Enum.IsDefined(typeof(TagNameProjectIcon), TagName))
        {
            name = (TagNameProjectIcon)TagName;
            return true;
        }

        name = TagNameProjectIcon.UNKNOWN;
        return false;
    }

    public override byte[] GetBytes()
    {
        // Ensure DataSize is up-to-date with table strings
        DataSize = CalcDataSize();

        // Now that data size is correct, stream can be created
        MemoryStream value = CreateMemoryStreamWithHeaderData();

        foreach (string icon in IconTable)
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
        return !IsInvalid;
    }

    public override bool IsFixedDataSize()
    {
        return false;
    }
};