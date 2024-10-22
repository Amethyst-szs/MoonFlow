using Godot;
using System;

using MessageStudio.Formats.BinaryText;

namespace Nindot
{
    public partial class MsbtFileAccess : GodotObject
    {
        public static Msbt ParseBytes(byte[] data)
        {
            return Msbt.FromBinary(data);
        }

        public static Msbt ParseFile(string path)
        {
            if (!FileAccess.FileExists(path))
                return [];
            
            byte[] data = FileAccess.GetFileAsBytes(path);
            return ParseBytes(data);
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