using System;
using System.IO;
using System.Linq;
using Godot;

using CommunityToolkit.HighPerformance;
using System.Collections.Generic;

namespace Nindot.LMS;

public class BlockHashTable : Block
{
    public struct HashTableLabel
    {
        public const int BASE_SIZE_WITHOUT_STRING = 0x5;

        public string Label;
        public uint ItemIndex;

        public HashTableLabel(byte[] labelData)
        {
            byte strLen = labelData[0];
            Label = labelData[1 .. (strLen + 1)].GetStringFromUtf8();

            ItemIndex = BitConverter.ToUInt32(labelData, strLen + 1);
        }

        public int CalcSizeBytes()
        {
            return Label.Length + BASE_SIZE_WITHOUT_STRING;
        }

        public void WriteLabel(ref MemoryStream stream)
        {
        }
    }

    public struct HashTableEntry
    {
        private uint LabelCount;
        private uint LabelOffset;
        public List<HashTableLabel> LabelList;

        public HashTableEntry(byte[] entryData)
        {
            LabelCount = BitConverter.ToUInt32(entryData, 0x0);
            LabelOffset = BitConverter.ToUInt32(entryData, 0x4);
        }

        public void InitLabelList(byte[] blockData)
        {
            // Setup a pointer into the data, starting at the offset written in the hash table
            int pointer = (int)LabelOffset;

            // Run a loop for the label count written in the table entry
            for (int i = 0; i < LabelCount; i++)
            {
                int strLen = blockData[pointer];
                int pointerEnd = pointer + strLen;

                HashTableLabel label = new(blockData[pointer..pointerEnd]);

                LabelList.Add(label);
                pointer += label.CalcSizeBytes();
            }
        }

        public void WriteEntry(ref MemoryStream stream)
        {
        }
    }


    public const int HASH_TABLE_ENTRY_SIZE = 0x8;

    protected uint SlotCount;
    protected List<HashTableEntry> HashEntryList;

    public BlockHashTable(byte[] data, string typeName) : base(data, typeName)
    {
    }

    protected override void InitBlock(byte[] data)
    {
        // Establish data pointer
        int pointer = 0;

        // Read slot count
        SlotCount = BitConverter.ToUInt32(data, pointer);
        pointer += sizeof(uint);

        // Iterate over all hash table entries
        for (int i = 0; i < SlotCount; i++)
        {
            int pointerEnd = pointer + HASH_TABLE_ENTRY_SIZE;
            byte[] segment = data[pointer..pointerEnd];
            HashEntryList.Add(new HashTableEntry(segment));

            pointer += HASH_TABLE_ENTRY_SIZE;
        }

        // Create label structure using newly built tables
        foreach (var entry in HashEntryList)
        {
            entry.InitLabelList(data);
        }
    }

    protected override uint CalcDataSize()
    {
        uint size = sizeof(uint); // 0x4 bytes are taken up by SlotCount

        foreach (var entry in HashEntryList)
        {
            // Add the size of the hash table element to the size counter
            size += HASH_TABLE_ENTRY_SIZE;

            foreach (var label in entry.LabelList)
            {
                size += (uint)label.CalcSizeBytes();
            }
        }

        return size;
    }

    protected override void WriteBlockData(ref MemoryStream stream)
    {
        throw new NotImplementedException();
    }

    public override bool IsValid()
    {
        if (!base.IsValid())
            return false;
        
        if (SlotCount == 0 || HashEntryList.Count != SlotCount)
            return false;
        
        return true;
    }

    protected static ulong CalcHash(string label, uint slotCount)
    {
        ulong hash = 0;

        foreach (char c in label)
        {
            hash = hash * 0x492 + (byte)c;
        }

        return (hash & 0xFFFFFFFF) % slotCount;
    }
}