//
// szs C# bindings
// - riidefi, 2023
//
// Crate page: https://crates.io/crates/szs
// Github: https://github.com/riidefi/RiiStudio/blob/master/source/szs/
//

using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Module.RiiStudioSzs;

public static class szs_native
{
  public const string DllName = "szs.dll";

  //
  // For the following APIs, keep in sync with `szs.h` (https://github.com/riidefi/RiiStudio/blob/master/source/szs/include/szs.h)
  //

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern uint riiszs_is_compressed(byte[] src, uint len);

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern uint riiszs_decoded_size(byte[] src, uint len);

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern IntPtr riiszs_decode(byte[] buf, uint len, byte[] src, uint src_len);

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern uint riiszs_encoded_upper_bound(uint len);

  public enum RiiszsEncodeAlgo
  {
    WorstCaseEncoding,
    Nintendo,
    MkwSP,
    CTGP,
    Haroohie,
    CTlib,
    LibYaz0,
    MK8,
  }

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern IntPtr riiszs_encode_algo_fast(IntPtr dst, uint dst_len, byte[] src, uint src_len, out uint used_len, uint algo);

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern void riiszs_free_error_message(string msg);

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern int szs_get_version_unstable_api(byte[] buf, uint len);
}

public static class Szs
{
  public enum CompressionAlgorithm
  {
    WorstCaseEncoding = szs_native.RiiszsEncodeAlgo.WorstCaseEncoding,
    Nintendo = szs_native.RiiszsEncodeAlgo.Nintendo,
    MkwSP = szs_native.RiiszsEncodeAlgo.MkwSP,
    CTGP = szs_native.RiiszsEncodeAlgo.CTGP,
    Haroohie = szs_native.RiiszsEncodeAlgo.Haroohie,
    CTlib = szs_native.RiiszsEncodeAlgo.CTlib,
    LibYaz0 = szs_native.RiiszsEncodeAlgo.LibYaz0,
    MK8 = szs_native.RiiszsEncodeAlgo.MK8,
  }

  private static bool IsValidWorkingDirectory = false;

  public static void InitWorkingDirectory()
  {
    if (IsValidWorkingDirectory) return;

    var cur = Directory.GetCurrentDirectory();
    var target = cur + '/' + szs_native.DllName;
    if (File.Exists(target))
    {
      IsValidWorkingDirectory = true;
      return;
    }

    // Attempt to navigate into .godot build path
    target = cur + "/.godot/mono/temp/bin/Debug/";
    if (!File.Exists(target + szs_native.DllName))
      throw new DllNotFoundException("Could not find " + target);

    Directory.SetCurrentDirectory(target);
    IsValidWorkingDirectory = true;
  }

  public static bool IsCompressed(byte[] data)
  {
    InitWorkingDirectory();
    return szs_native.riiszs_is_compressed(data, (uint)data.Length) != 0;
  }

  public static byte[] Decode(byte[] compressedData)
  {
    InitWorkingDirectory();
    if (!IsCompressed(compressedData))
    {
      throw new ArgumentException("Data is not SZS (\"YAZ0\" or \"YAZ1\") compressed.");
    }
    uint expanded = szs_native.riiszs_decoded_size(compressedData, (uint)compressedData.Length);
    byte[] decodedData = new byte[expanded];

    IntPtr errorPtr = szs_native.riiszs_decode(decodedData, (uint)decodedData.Length, compressedData, (uint)compressedData.Length);

    // Check for errors
    if (errorPtr != IntPtr.Zero)
    {
      // Convert error string pointer to a C# string
      string errorMsg = Marshal.PtrToStringAnsi(errorPtr);
      szs_native.riiszs_free_error_message(errorMsg);
      throw new InvalidOperationException($"Encoding failed: {errorMsg}");
    }

    return decodedData;
  }

  public static byte[] Encode(byte[] data, CompressionAlgorithm algorithm = CompressionAlgorithm.MK8)
  {
    InitWorkingDirectory();
    uint upperBound = szs_native.riiszs_encoded_upper_bound((uint)data.Length);
    IntPtr encodedPtr = Marshal.AllocHGlobal((int)upperBound);
    try
    {
      uint usedLen;
      IntPtr errorPtr = szs_native.riiszs_encode_algo_fast(encodedPtr, upperBound, data, (uint)data.Length, out usedLen, (uint)algorithm);

      // Check for errors
      if (errorPtr != IntPtr.Zero)
      {
        // Convert error string pointer to a C# string
        string errorMsg = Marshal.PtrToStringAnsi(errorPtr);
        szs_native.riiszs_free_error_message(errorMsg);
        throw new InvalidOperationException($"Encoding failed: {errorMsg}");
      }

      byte[] encodedData = new byte[usedLen];
      Marshal.Copy(encodedPtr, encodedData, 0, (int)usedLen);

      return encodedData;
    }
    finally
    {
      Marshal.FreeHGlobal(encodedPtr);
    }
  }
  public static string GetVersion()
  {
    InitWorkingDirectory();
    byte[] buffer = new byte[256];
    int length = szs_native.szs_get_version_unstable_api(buffer, (uint)buffer.Length);
    if (length > 0 && length < 256)
    {
      return Encoding.UTF8.GetString(buffer, 0, length);
    }
    else
    {
      throw new InvalidOperationException("Failed to retrieve version.");
    }
  }
}
