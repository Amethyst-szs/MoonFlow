using System;
using System.Collections.Generic;
using System.IO;
using Godot;

namespace Nindot.LMS;

public abstract class FileBase
{
    protected FileHeader Header;
    protected List<Block> Blocks = [];

    public FileBase(byte[] data)
    {
        // Load header and ensure validity before continuing
        Header = new(data, GetFileMagic());
        if (!Header.IsValid())
            return;

        // Create a list of block keys and offsets into binary
        Dictionary<string, int> blockKeys = [];

        int pointer = FileHeader.HEADER_SIZE;
        while (pointer < data.Length)
        {
            // Read the first four bytes of the block (name) and create dict key
            string key = data[pointer .. (pointer + Block.TYPE_NAME_SIZE)].GetStringFromUtf8();
            blockKeys.Add(key, pointer);
            pointer += Block.TYPE_NAME_SIZE;

            // Read the size of the block so we know where to go next
            uint dataSize = BitConverter.ToUInt32(data, pointer);
            pointer += sizeof(uint);

            // Advance past padding, skip dataSize, and align with next alignment grid
            pointer += (int)(Block.PADDING_SIZE + dataSize);
            if (pointer % Block.BLOCK_ALIGNMENT_SIZE != 0)
                pointer += Block.BLOCK_ALIGNMENT_SIZE - (pointer % Block.BLOCK_ALIGNMENT_SIZE);

            continue;
        }

        // Call abstract initalization function, handle file extension specific things like blocks here
        Init(blockKeys);
    }

    public abstract void Init(Dictionary<string, int> blockKeys);

    public virtual void WriteFile(ref MemoryStream stream)
    {

    }

    public abstract string GetFileMagic();

    public bool IsValid()
    {
        if (!Header.IsValid())
            return false;
        
        if (Blocks.Count == 0)
            return false;

        foreach (var block in Blocks)
        {
            if (!block.IsValid())
                return false;
        }

        return true;
    }
}