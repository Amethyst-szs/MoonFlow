using Godot;

using MessageStudio.Formats.BinaryText;
using SarcLibrary;

namespace Nindot
{
    [GlobalClass]
    public partial class MsbtResource : Resource
    {
        public LMS.Msbt.TagLib.Core.Type TagLib = LMS.Msbt.TagLib.Core.Type.NONE;

        public LMS.Msbt.Content Content;

        // Constructor and Initilzation Functions

        public MsbtResource()
        {
            TagLib = LMS.Msbt.TagLib.Core.Type.NONE;
            Content = null;
        }

        public MsbtResource(Msbt file, LMS.Msbt.TagLib.Core.Type taglib)
        {
            TagLib = taglib;
            Content = new LMS.Msbt.Content(file, taglib);
        }

        static public MsbtResource FromFilePath(string path, LMS.Msbt.TagLib.Core.Type tagLib)
        {
            Msbt msbt;
            Error err = MsbtFileAccess.ParseFile(out msbt, path);

            if (err == Error.Ok)
                return new MsbtResource(msbt, tagLib);
            else
                return null;
        }

        static public MsbtResource FromBytes(byte[] data, LMS.Msbt.TagLib.Core.Type tagLib)
        {
            Msbt msbt;
            Error err = MsbtFileAccess.ParseBytes(out msbt, data);

            if (err == Error.Ok)
                return new MsbtResource(msbt, tagLib);
            else
                return null;
        }

        static public MsbtResource FromSarc(SarcResource sarc, string file, LMS.Msbt.TagLib.Core.Type tagLib)
        {
            Sarc archive = sarc.SarcDict;
            if (!archive.ContainsKey(file))
                return null;

            byte[] data = archive[file].ToArray();

            Msbt msbt;
            Error err = MsbtFileAccess.ParseBytes(out msbt, data);

            if (err == Error.Ok)
                return new MsbtResource(msbt, tagLib);
            else
                return null;
        }

        // GDScript Interfaces

        public bool IsValid()
        {
            return Content != null && TagLib != LMS.Msbt.TagLib.Core.Type.ENUM_SIZE;
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