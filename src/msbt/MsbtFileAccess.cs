using System;
using Godot;

using MessageStudio.Formats.BinaryText;

namespace Nindot
{
    public partial class MsbtFileAccess : GodotObject
    {
        public static Error ParseBytes(out Msbt msbt, byte[] data)
        {
            // Ensure that this data starts with the file signature/magic
            if (BitConverter.ToUInt64(data, 0) != Msbt.MAGIC) {
                msbt = null;
                return Error.FileCantOpen;
            }

            // Use msbt library to read the binary
            msbt = Msbt.FromBinary(data);
            return Error.Ok;
        }

        public static Error ParseFile(out Msbt msbt, string path)
        {
            if (!FileAccess.FileExists(path)) {
                msbt = null;
                return Error.FileNotFound;
            }
            
            byte[] data = FileAccess.GetFileAsBytes(path);
            ParseBytes(out msbt, data);
            return Error.Ok;
        }

        public static byte[] WriteBytes(Msbt msbt)
        {
            return msbt.ToBinary();
        }

        public static bool WriteDisk(string path, Msbt msbt)
        {
            // Ensure path is valid
            if (!DirAccess.DirExistsAbsolute(path.GetBaseDir()))
                return false;
            
            // Get byte array from msbt
            byte[] data = WriteBytes(msbt);

            // Write bytes to disk
            FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
            file.StoreBuffer(data);
            file.Close();

            return true;
        }
    }
}