using Godot;

using CsYaz0;
using SarcLibrary;

namespace Nindot
{
    public partial class SarcFileAccess : GodotObject
    {
        public static bool ParseBytes(out Sarc sarc, byte[] fileCompressed)
        {
            // Decompress file using Yaz0, and return early if this fails
            byte[] bytes = Yaz0.Decompress(fileCompressed);
            if (bytes.IsEmpty())
            {
                sarc = new Sarc();
                return false;
            }

            // Convert this decompressed file into a sarc object, and return a failure if empty
            sarc = Sarc.FromBinary(bytes);
            if (sarc.Count == 0)
                return false;

            return true;
        }

        public static bool ParseFile(out Sarc sarc, string path)
        {
            // Ensure the file exists, returning early if not
            if (!FileAccess.FileExists(path))
            {
                sarc = new Sarc();
                return false;
            }

            byte[] file = FileAccess.GetFileAsBytes(path);
            ParseBytes(out sarc, file);

            return true;
        }

        public static byte[] WriteBytes(Sarc sarc)
        {
            // Convert sarc object to memory stream
            System.IO.MemoryStream stream = new();
            sarc.Write(stream);

            // Compress this memory stream using Yaz0
            CsYaz0.Marshalling.DataMarshal compress = Yaz0.Compress(stream.ToArray());

            // Return compressed version as byte array
            return compress.ToArray();
        }

        public static bool WriteDisk(Sarc sarc, string path)
        {
            // Ensure path is valid
            if (!DirAccess.DirExistsAbsolute(path.GetBaseDir()))
                return false;

            // Get byte array of compressed sarc file
            byte[] stream = WriteBytes(sarc);

            // Write byte array to disk
            FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
            file.StoreBuffer(stream);
            file.Close();

            return true;
        }
    }
}