using System;
using System.IO;
using System.Linq;

using CommunityToolkit.HighPerformance;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Nindot.LMS;

public class BlockHashTable(byte[] data, string typeName, int offset, FileBase parent) : Block(data, typeName, offset, parent)
{
    // ~~~~~~~~ Sub-Class Definitions ~~~~~~~~ //
    public class HashTableLabel
    {
        public const int BASE_SIZE_WITHOUT_STRING = 0x5;

        public string Label;
        public uint ItemIndex;

        public HashTableLabel(byte[] labelData)
        {
            byte strLen = labelData[0];
            Label = Encoding.UTF8.GetString(labelData[1..(strLen + 1)]);

            ItemIndex = BitConverter.ToUInt32(labelData, strLen + 1);
        }

        public HashTableLabel(string name, uint index)
        {
            Label = name;
            ItemIndex = index;
        }

        public int CalcSizeBytes()
        {
            return Label.Length + BASE_SIZE_WITHOUT_STRING;
        }
    }

    public struct HashTableEntry
    {
        public const int ENTRY_SIZE = 0x8;

        private readonly uint _initdata_LabelCount;
        private readonly uint _initdata_LabelOffset;
        public List<HashTableLabel> LabelList = [];

        public HashTableEntry(byte[] entryData)
        {
            _initdata_LabelCount = BitConverter.ToUInt32(entryData, 0x0);
            _initdata_LabelOffset = BitConverter.ToUInt32(entryData, 0x4);
        }

        public void InitLabelList(byte[] blockData)
        {
            // Setup a pointer into the data, starting at the offset written in the hash table
            int pointer = (int)_initdata_LabelOffset;

            // Run a loop for the label count written in the table entry
            for (int i = 0; i < _initdata_LabelCount; i++)
            {
                int strLen = blockData[pointer];
                int pointerEnd = pointer + strLen + HashTableLabel.BASE_SIZE_WITHOUT_STRING;

                HashTableLabel label = new(blockData[pointer..pointerEnd]);

                LabelList.Add(label);
                pointer += label.CalcSizeBytes();
            }
        }

        public void EmptyLabelList()
        {
            LabelList.Clear();
        }
    }

    // ~~~~ Properties of Hash Table block ~~~ //

    public const int HASH_TABLE_ENTRY_SIZE = 0x8;

    protected uint SlotCount;
    protected List<HashTableEntry> HashEntryList = [];

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
        uint size = sizeof(uint); // 0x4 bytes are taken up by slot count

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

    protected override void WriteBlockData(MemoryStream stream)
    {
        // Write how large the hash table is
        stream.Write((uint)HashEntryList.Count);

        // Create an int for how large the hash table bytes will be, before any labels
        int hashTableByteSize = (HashEntryList.Count * HashTableEntry.ENTRY_SIZE) + 0x4;

        // Build a memory stream of the labels, will append to end of main stream at end of func
        MemoryStream labelStream = new(HashEntryList.Count * HashTableEntry.ENTRY_SIZE);

        // Iterate over every hash table, building both the table slot stream and label stream
        foreach (var table in HashEntryList)
        {
            // Write the current hash table label count and offset
            stream.Write((uint)table.LabelList.Count);
            stream.Write((uint)(labelStream.Position + hashTableByteSize));

            // Write all the labels in this hash tabel entry to labelStream
            foreach (var label in table.LabelList)
            {
                labelStream.Write((byte)label.Label.Length);
                labelStream.Write(Encoding.UTF8.GetBytes(label.Label));
                labelStream.Write(label.ItemIndex);
            }
        }

        // And append the labels to the end of the main stream
        stream.Write(labelStream.ToArray());
        return;
    }

    public void RebuildTable(string[] labels)
    {
        // Ensure the block has a valid header
        if (!IsBlockHeaderOK)
            return;

        // Empty out the hash table of its label contents
        foreach (var entry in HashEntryList)
        {
            entry.EmptyLabelList();
        }

        // Run through every label in the user-provided list, and insert them
        // into their coresponding hash table entries
        for (uint labelIdx = 0; labelIdx < labels.Length; labelIdx++)
        {
            string label = labels[labelIdx];
            ulong hash = CalcHash(label, SlotCount);

            HashTableLabel labelEntry = new(label, labelIdx);
            HashEntryList[(int)hash].LabelList.Add(labelEntry);
        }
    }

    // ====================================================== //
    // === Calculation and Getter utilities for LMS files === //
    // ====================================================== //

    public ulong CalcHash(string label)
    {
        return CalcHash(label, SlotCount);
    }

    public static ulong CalcHash(string label, uint slotCount)
    {
        ulong hash = 0;

        foreach (char c in label)
        {
            hash = hash * 0x492 + (byte)c;
        }

        return (hash & 0xFFFFFFFF) % slotCount;
    }

    public int GetHashTableSize()
    {
        return HashEntryList.Count;
    }

    public int CalcLabelCount()
    {
        int count = 0;

        foreach (var table in HashEntryList)
        {
            count += table.LabelList.Count;
        }

        return count;
    }

    public ReadOnlyCollection<string> GetLabelList()
    {
        string[] list = new string[CalcLabelCount()];

        foreach (var table in HashEntryList)
        {
            foreach (var label in table.LabelList)
            {
                list[label.ItemIndex] = label.Label;
            }
        }

        return new ReadOnlyCollection<string>(list);
    }

    public ReadOnlyCollection<HashTableLabel> GetRawLabelList()
    {
        HashTableLabel[] list = new HashTableLabel[CalcLabelCount()];

        foreach (var table in HashEntryList)
        {
            foreach (var label in table.LabelList)
            {
                list[label.ItemIndex] = label;
            }
        }

        return new ReadOnlyCollection<HashTableLabel>(list);
    }

    public int GetItemIndex(string labelName)
    {
        ulong hash = CalcHash(labelName);
        HashTableEntry table = HashEntryList.ElementAt((int)hash);

        return GetItemIndex(labelName, table);
    }

    public static int GetItemIndex(string labelName, HashTableEntry table)
    {
        for (int i = 0; i < table.LabelList.Count; i++)
        {
            HashTableLabel cmp = table.LabelList[i];

            if (cmp.Label.Length != labelName.Length)
                continue;

            if (cmp.Label == labelName)
                return (int)cmp.ItemIndex;
        }

        return -1;
    }

    public HashTableLabel GetItem(string labelName)
    {
        ulong hash = CalcHash(labelName);
        HashTableEntry table = HashEntryList.ElementAt((int)hash);

        return GetItem(labelName, table);
    }

    public static HashTableLabel GetItem(string labelName, HashTableEntry table)
    {
        for (int i = 0; i < table.LabelList.Count; i++)
        {
            HashTableLabel cmp = table.LabelList[i];

            if (cmp.Label.Length != labelName.Length)
                continue;

            if (cmp.Label == labelName)
                return cmp;
        }

        return null;
    }

    // ====================================================== //
    // ========== Hash Table modification utilities ========= //
    // ====================================================== //

    internal void AddItem(string labelName, int index)
    {
        ulong hash = CalcHash(labelName);
        HashTableEntry table = HashEntryList.ElementAt((int)hash);

        HashTableLabel label = new(labelName, (uint)index);
        table.LabelList.Add(label);
    }

    internal void MoveItem(string labelName, int newIndex)
    {
        ulong hash = CalcHash(labelName);
        HashTableEntry table = HashEntryList.ElementAt((int)hash);

        HashTableLabel label = GetItem(labelName, table);
        label.ItemIndex = (uint)newIndex;
    }

    internal void MoveItemByOffset(string labelName, int offset)
    {
        ulong hash = CalcHash(labelName);
        HashTableEntry table = HashEntryList.ElementAt((int)hash);

        int index = GetItemIndex(labelName, table);
        if (index == -1)
            return;

        MoveItem(labelName, index + offset);
    }

    // Returns the index of the removed item
    internal int RemoveItem(string labelName)
    {
        ulong hash = CalcHash(labelName);
        HashTableEntry table = HashEntryList.ElementAt((int)hash);

        HashTableLabel label = GetItem(labelName, table);
        if (label == null)
            return -1;

        table.LabelList.Remove(label);
        return (int)label.ItemIndex;
    }
}