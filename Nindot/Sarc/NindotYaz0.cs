using System.IO;
using System.IO.Compression;

using AuroraLib.Compression.Algorithms;
using AuroraLib.Core.IO;

namespace Nindot;

public static class NindotYaz0
{
    private const CompressionLevel CompressionType = CompressionLevel.Fastest;

    public static byte[] Compress(byte[] source)
    {
        var result = new MemoryStream();
        new Yaz0() { LookAhead = true }.Compress(source, result, CompressionType);

        return result.ToArray();
    }
    public static byte[] Compress(Stream source)
    {
        var result = new MemoryStream();
        new Yaz0() { LookAhead = true }.Compress(source.ToArray(), result, CompressionType);

        return result.ToArray();
    }
    public static void Compress(byte[] source, Stream result)
    {
        new Yaz0() { LookAhead = true }.Compress(source, result, CompressionType);
    }
    public static void Compress(Stream source, Stream result)
    {
        new Yaz0() { LookAhead = true }.Compress(source.ToArray(), result, CompressionType);
    }

    public static byte[] Decompress(byte[] source)
    {
        return new Yaz0().Decompress(source);
    }
    public static byte[] Decompress(Stream source)
    {
        return new Yaz0().Decompress(source.ToArray());
    }
    public static void Decompress(byte[] source, Stream result)
    {
        new Yaz0().Decompress(source, result);
    }
    public static void Decompress(Stream source, Stream result)
    {
        new Yaz0().Decompress(source.ToArray(), result);
    }
}