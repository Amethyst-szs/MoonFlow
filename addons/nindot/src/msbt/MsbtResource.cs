using Godot;

using MessageStudio.Formats.BinaryText;
using SarcLibrary;

namespace Nindot
{
    [GlobalClass]
    public partial class MsbtResource : Resource
    {
        public MsbtTagLibrary.Core.Type TagLib = MsbtTagLibrary.Core.Type.NONE;

        public MsbtContent.Content Content;

        // Constructor and Initilzation Functions

        public MsbtResource()
        {
            TagLib = MsbtTagLibrary.Core.Type.NONE;
            Content = null;
        }

        public MsbtResource(Msbt file, MsbtTagLibrary.Core.Type taglib)
        {
            TagLib = taglib;
            Content = new MsbtContent.Content(file, taglib);
        }

        static public MsbtResource FromFilePath(string path, MsbtTagLibrary.Core.Type tagLib)
        {
            Msbt msbt = MsbtFileAccess.ParseFile(path);
            return new MsbtResource(msbt, tagLib);
        }

        static public MsbtResource FromSarc(SarcResource sarc, string file, MsbtTagLibrary.Core.Type tagLib)
        {
            Sarc archive = sarc.SarcDict;
            if (!archive.ContainsKey(file))
                return null;

            byte[] data = archive[file].ToArray();

            Msbt msbt = MsbtFileAccess.ParseBytes(data);
            return new MsbtResource(msbt, tagLib);
        }

        // GDScript Interfaces

        public bool IsValid()
        {
            return Content != null && TagLib != MsbtTagLibrary.Core.Type.ENUM_SIZE;
        }

        public int GetKeyCount()
        {
            return Content.Count;
        }

        public string[] GetKeys()
        {
            string[] keys = new string[Content.Count];
            Content.Keys.CopyTo(keys, 0);

            return keys;
        }
    }
}