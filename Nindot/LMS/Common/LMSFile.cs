using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nindot.LMS;

public abstract class FileBase
{
    public string Name { get; private set; } = null;
    protected FileHeader Header;
    protected List<Block> Blocks = [];

    public FileBase(byte[] data, string name)
    {
        // Set name
        Name = name;
        
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
            string key = Encoding.UTF8.GetString(data[pointer..(pointer + Block.TYPE_NAME_SIZE)]);
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
        Init(data, blockKeys);
    }

    public abstract void Init(byte[] data, Dictionary<string, int> blockKeys);
    public abstract string GetFileMagic();

    public virtual bool WriteFile(MemoryStream stream)
    {
        if (!Header.WriteHeader(stream))
            return false;
        
        foreach (var b in Blocks)
        {
            if (!b.WriteBlock(stream))
                return false;
        }

        return true;
    }

    public bool IsValid()
    {
        return Header.IsValid();
    }
}

public class LMSException(string error) : Exception(error);