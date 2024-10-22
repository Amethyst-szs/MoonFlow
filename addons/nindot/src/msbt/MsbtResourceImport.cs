using Godot;
using Godot.Collections;
using MessageStudio.Formats.BinaryText;

namespace Nindot
{
    [Tool]
    public partial class MsbtResourceImport : EditorImportPlugin
    {
        public override Error _Import(string sourceFile, string savePath, Dictionary options, Array<string> platformVariants, Array<string> genFiles)
        {
            // Ensure source file path is valid
            if (!FileAccess.FileExists(sourceFile))
                return Error.FileNotFound;

            // Generate sarc from file path
            Msbt msbt = MsbtFileAccess.ParseFile(sourceFile);

            // Create a SarcResource class
            MsbtResource res = new();
            
            // Write new resource file using resource saver
            string saveFile = string.Format("{0}.{1}", savePath, _GetSaveExtension());
            Error value = ResourceSaver.Save(res, saveFile);

            if (value != Error.Ok)
                GD.PushError("Failed to import MSBT in ResourceSaver: " + value.ToString());
            
            return value;
        }

        public override string _GetImporterName()
        {
            return "nindot.msbt";
        }

        public override string _GetVisibleName()
        {
            return "Message Studio Binary Text";
        }

        public override string[] _GetRecognizedExtensions()
        {
            return ["msbt"];
        }

        public override string _GetSaveExtension()
        {
            return "tres";
        }

        public override string _GetResourceType()
        {
            return "MsbtResource";
        }

        public override float _GetPriority()
        {
            return 1;
        }

        public override int _GetPresetCount()
        {
            return 1;
        }

        public override string _GetPresetName(int presetIndex)
        {
            return "Default";
        }

        public override Array<Dictionary> _GetImportOptions(string path, int presetIndex)
        {
            return [];
        }

        public override bool _GetOptionVisibility(string path, StringName optionName, Dictionary options)
        {
            return true;
        }

        public override int _GetImportOrder()
        {
            return 0;
        }

        public override bool _CanImportThreaded()
        {
            return true;
        }
    }
}