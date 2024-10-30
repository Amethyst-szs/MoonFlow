using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbp;

public class BlockStyles : Block
{
    public class Style
    {
        public const int STYLE_STRUCT_SIZE = 0x10;

        public uint RegionWidth;
        public uint LineNumber;
        public uint FontIndex;
        public uint DefaultColorIndex;

        public Style(byte[] entryData)
        {
            RegionWidth = BitConverter.ToUInt32(entryData, 0x0);
            LineNumber = BitConverter.ToUInt32(entryData, 0x4);
            FontIndex = BitConverter.ToUInt32(entryData, 0x8);
            DefaultColorIndex = BitConverter.ToUInt32(entryData, 0xC);
        }

        public Style(uint regionWidth, uint lineNumber, uint fontIndex, uint colorIndex)
        {
            RegionWidth = regionWidth;
            LineNumber = lineNumber;
            FontIndex = fontIndex;
            DefaultColorIndex = colorIndex;
        }

        public void Write(MemoryStream stream)
        {
            stream.Write(RegionWidth);
            stream.Write(LineNumber);
            stream.Write(FontIndex);
            stream.Write(DefaultColorIndex);
        }
    }

    private List<Style> Styles = [];

    public BlockStyles(byte[] data, string typeName, int offset) : base(data, typeName, offset)
    {
    }

    protected override void InitBlock(byte[] data)
    {
        uint styleCount = BitConverter.ToUInt32(data, 0x0);

        for (int i = 0; i < styleCount; i++)
        {
            int pointer = (i * Style.STYLE_STRUCT_SIZE) + 0x4;
            Style s = new Style(data[pointer..(pointer + Style.STYLE_STRUCT_SIZE)]);
            Styles.Add(s);
        }
    }

    protected override uint CalcDataSize()
    {
        return (uint)(Styles.Count * Style.STYLE_STRUCT_SIZE) + sizeof(uint);
    }

    protected override void WriteBlockData(MemoryStream stream)
    {
        stream.Write((uint)Styles.Count);

        foreach (var style in Styles)
        {
            style.Write(stream);
        }
    }

    public ReadOnlyCollection<Style> GetStyleList()
    {
        return new ReadOnlyCollection<Style>(Styles);
    }

    public Style GetStyle(int idx)
    {
        if (idx >= Styles.Count)
            return null;

        return Styles[idx];
    }

    internal int AddStyle(Style s)
    {
        Styles.Add(s);
        return Styles.IndexOf(s);
    }

    internal void MoveStyle(int startIndex, int endIndex)
    {
        // Ensure start and end index are both within the bounds of the list
        if (startIndex < 0 || startIndex >= Styles.Count || endIndex < 0 || endIndex >= Styles.Count)
            return;

        Style c = Styles[startIndex];
        Styles.Remove(c);
        Styles.Insert(endIndex, c);
    }

    internal void RemoveStyle(int idx)
    {
        if (idx >= Styles.Count)
            return;

        Styles.RemoveAt(idx);
    }
}