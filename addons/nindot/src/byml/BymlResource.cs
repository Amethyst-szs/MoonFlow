using Godot;

using SarcLibrary;

namespace Nindot
{
    [GlobalClass]
    public partial class BymlResource : Resource
    {
        public BymlIter Iter;

        public BymlResource(BymlIter file)
        {
            Iter = file;
        }

        static public BymlResource FromFilePath(string path)
        {
            BymlIter byml;
            if (!BymlFileAccess.ParseFile(out byml, path))
                return null;

            return new BymlResource(byml);
        }

        static public BymlResource FromSarc(SarcResource sarc, string file)
        {
            Sarc archive = sarc.SarcDict;
            if (!archive.ContainsKey(file))
                return null;

            byte[] data = archive[file].ToArray();

            BymlIter byml;
            if (!BymlFileAccess.ParseBytes(out byml, data))
                return null;

            return new BymlResource(byml);
        }

        // GDScript Interfaces

        public int GetKeyCount()
        {
            return Iter.Count;
        }

        public string[] GetKeys()
        {
            string[] keys = new string[Iter.Count];
            Iter.Keys.CopyTo(keys, 0);

            return keys;
        }
    }
}