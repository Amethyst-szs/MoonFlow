using System;
using System.IO;

using CommunityToolkit.HighPerformance;
using System.Text;

namespace Nindot.LMS;

public class FileHeader
{
    public enum Endianness : ushort
    {
        BIG = 0xFEFF,
        LITTLE = 0xFFFE,
        INVALID = 0xFFFF,
    }

    public enum StringEncoding : byte
    {
        UTF8 = 0,
        UTF16 = 1,
        UTF32 = 2,
        INVALID = 0xFF,
    }

    public const ushort HEADER_SIZE = 0x20;
    public const ushort MAGIC_SIZE = 0x8;
    public const ushort PADDING_SIZE = 0xA;

    protected readonly string Magic = ""; // 0x0
    protected readonly Endianness Endian = Endianness.INVALID; // 0x8
    private readonly ushort Unknown; // 0xA
    protected readonly StringEncoding EncodeType = StringEncoding.INVALID; // 0xC
    protected readonly byte Version; // 0xD
    protected ushort BlockCount; //0xE
    private readonly ushort Unknown2; // 0x10
    protected uint FileSize; // 0x12
    
    // 0xA bytes of padding

    public FileHeader(byte[] data, string requiredMagic = "n/a")
    {
        // Ensure data is large enough to include header
        if (data.Length < HEADER_SIZE)
            throw new LMSException("LMS header read failed, byte data too short");

        // Setup pointer
        int pointer = 0;
        
        // MAGIC
        Magic = Encoding.UTF8.GetString(data[pointer..MAGIC_SIZE]);
        if (Magic != requiredMagic && requiredMagic != "n/a")
            throw new LMSException("LMS header has bad magic!");

        pointer += MAGIC_SIZE;

        // ENDIAN
        ushort endianKey = BitConverter.ToUInt16(data, pointer);
        if (Enum.IsDefined(typeof(Endianness), endianKey))
            Endian = (Endianness)endianKey;
        
        pointer += sizeof(Endianness);

        // UNKNOWN VALUE 1
        Unknown = BitConverter.ToUInt16(data, pointer);
        pointer += sizeof(ushort);

        // STRING ENCODING
        byte encodingKey = data[pointer];
        if (Enum.IsDefined(typeof(StringEncoding), encodingKey))
            EncodeType = (StringEncoding)encodingKey;
        
        pointer += sizeof(byte);

        // VERSION
        Version = data[pointer];
        pointer += sizeof(byte);

        // BLOCK COUNT
        BlockCount = BitConverter.ToUInt16(data, pointer);
        pointer += sizeof(ushort);

        // UNKNOWN VALUE 2
        Unknown2 = BitConverter.ToUInt16(data, pointer);
        pointer += sizeof(ushort);
        
        // FILE SIZE
        FileSize = BitConverter.ToUInt32(data, pointer);

        return;
    }

    public bool WriteHeader(MemoryStream stream)
    {
        if (!IsValid())
            return false;
        
        // Build binary stream data
        stream.Write(Encoding.UTF8.GetBytes(Magic));
        stream.Write(Endian);
        stream.Write(Unknown);
        stream.Write(EncodeType);
        stream.Write(Version);
        stream.Write(BlockCount);
        stream.Write(Unknown2);
        stream.Write(FileSize);
        
        // Pad out to correct size
        byte[] pad = new byte[PADDING_SIZE];
        stream.Write(pad);

        if (stream.Length != HEADER_SIZE)
            return false;

        return true;
    }

    public bool IsValid()
    {
        if (Magic.Length != MAGIC_SIZE)
            return false;
        
        if (Endian == Endianness.INVALID)
            return false;
        
        if (EncodeType == StringEncoding.INVALID)
            return false;
        
        return true;
    }

    public StringEncoding GetStrEncoding()
    {
        return EncodeType;
    }

    public byte[] GetStrAsBytes(string str)
    {
        return EncodeType switch
        {
            StringEncoding.UTF8 => Encoding.UTF8.GetBytes(str),
            StringEncoding.UTF16 => Encoding.Unicode.GetBytes(str),
            StringEncoding.UTF32 => Encoding.UTF32.GetBytes(str),
            _ => [],
        };
    }

    public string GetBytesAsStr(byte[] bytes)
    {
        return EncodeType switch
        {
            StringEncoding.UTF8 => Encoding.UTF8.GetString(bytes),
            StringEncoding.UTF16 => Encoding.Unicode.GetString(bytes),
            StringEncoding.UTF32 => Encoding.UTF32.GetString(bytes),
            _ => null,
        };
    }

    public int GetCharSize()
    {
        return EncodeType switch
        {
            StringEncoding.UTF8 => 1,
            StringEncoding.UTF16 => 2,
            StringEncoding.UTF32 => 4,
            _ => 0,
        };
    }
}