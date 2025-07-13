using System;
using System.IO;
using System.IO.Compression;

using AuroraLib.Compression.Algorithms;
using AuroraLib.Core.IO;

namespace Nindot;

public class NindotYaz0 : Yaz0
{
    private const CompressionLevel CompressionType = CompressionLevel.Optimal;

    public NindotYaz0() : base()
    {
        LookAhead = false;
        MemoryAlignment = 0x80;
    }

    public byte[] Compress(byte[] source)
    {
        var result = new MemoryStream();
        Compress(source, result, CompressionType);
        return result.ToArray();
    }
    public byte[] Compress(Stream source)
    {
        var result = new MemoryStream();
        Compress(source.ToArray(), result, CompressionType);
        return result.ToArray();
    }
}