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
    }
}