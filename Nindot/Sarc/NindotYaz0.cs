using System.IO;

using AuroraLib.Compression.Algorithms;
using AuroraLib.Core.IO;

namespace Nindot;

public static class NindotYaz0
{
    public static byte[] Compress(byte[] source)
    {
        var result = new MemoryStream();
        new Yaz0() { LookAhead = false }.Compress(source, result);

        return result.ToArray();
    }
    public static byte[] Compress(Stream source)
    {
        var result = new MemoryStream();
        new Yaz0() { LookAhead = false }.Compress(source.ToArray(), result);

        return result.ToArray();
    }
    public static void Compress(byte[] source, Stream result)
    {
        new Yaz0() { LookAhead = false }.Compress(source, result);
    }
    public static void Compress(Stream source, Stream result)
    {
        new Yaz0() { LookAhead = false }.Compress(source.ToArray(), result);
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