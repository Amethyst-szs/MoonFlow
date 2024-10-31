using Godot;
using SarcLibrary;
using System;

namespace Nindot
{
    [GlobalClass]
    public partial class SarcResource : Resource
    {
        public Sarc SarcDict;

        public SarcResource(Sarc file)
        {
            SarcDict = file;
        }

        static public SarcResource FromFilePath(string path)
        {
            Sarc sarc;
            if (!SarcFileAccess.ParseFile(out sarc, path))
                return null;

            return new SarcResource(sarc);
        }

        // GDScript compatible interfaces

        public int GetFileCount()
        {
            return SarcDict.Count;
        }

        public string[] GetFileList()
        {
            string[] keys = new string[SarcDict.Count];
            SarcDict.Keys.CopyTo(keys, 0);

            return keys;
        }

        public byte[] GetFile(string name)
        {
            return SarcDict[name].ToArray();
        }

        // public MsbtResource GetFileMsbt(string name, LMS.Msbt.TagLib.Core.Type tagLib)
        // {
        //     return MsbtResource.FromSarc(this, name, tagLib);
        // }

        public BymlResource GetFileByml(string name)
        {
            return BymlResource.FromSarc(this, name);
        }
    }
}