// using System;
// using Godot;

// using MessageStudio.Formats.BinaryText;

// namespace Nindot
// {
//     public partial class MsbtFileAccess : GodotObject
//     {
//         public static Error ParseBytes(out Msbt msbt, byte[] data)
//         {
//             // Ensure that this data starts with the file signature/magic
//             if (BitConverter.ToUInt64(data, 0) != Msbt.MAGIC) {
//                 msbt = null;
//                 return Error.FileCantOpen;
//             }

//             // Use msbt library to read the binary
//             MsbtOptions opt = new()
//             {
//                 IsGenerateTextStrings = false
//             };

//             msbt = Msbt.FromBinary(data, opt);
//             return Error.Ok;
//         }

//         public static Error ParseFile(out Msbt msbt, string path)
//         {
//             if (!FileAccess.FileExists(path)) {
//                 msbt = null;
//                 return Error.FileNotFound;
//             }
            
//             byte[] data = FileAccess.GetFileAsBytes(path);
//             ParseBytes(out msbt, data);
//             return Error.Ok;
//         }

//         public static byte[] WriteBytes(LMS.Msbt.Content msbt)
//         {
//             System.IO.MemoryStream stream = new();

//             NindotWriter writer = new();
//             writer.ToBinary(stream, msbt.GenerateToBinaryDictionary());

//             return stream.ToArray();
//         }

//         public static bool WriteDisk(string path, LMS.Msbt.Content msbt)
//         {
//             // Ensure path is valid
//             if (!DirAccess.DirExistsAbsolute(path.GetBaseDir()))
//                 return false;
            
//             // Get byte array from msbt
//             byte[] data = WriteBytes(msbt);

//             // Write bytes to disk
//             FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
//             file.StoreBuffer(data);
//             file.Close();

//             return true;
//         }

//         public static bool WriteDisk(string path, byte[] data)
//         {
//             // Write bytes to disk
//             FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
//             file.StoreBuffer(data);
//             file.Close();

//             return true;
//         }
//     }
// }