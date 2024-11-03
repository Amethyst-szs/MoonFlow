#if TOOLS

using Godot;
using Godot.Collections;
using SarcLibrary;

namespace Nindot;

[Tool]
[GlobalClass]
public partial class SarcResourceImport : EditorImportPlugin
{
    public override Error _Import(string sourceFile, string savePath, Dictionary options, Array<string> platformVariants, Array<string> genFiles)
    {
        // Ensure source file path is valid
        if (!FileAccess.FileExists(sourceFile))
            return Error.FileNotFound;

        // Create a SarcResource class
        SarcResource res = SarcResource.FromFilePath(sourceFile);

        // Write new resource file using resource saver
        string saveFile = string.Format("{0}.{1}", savePath, _GetSaveExtension());
        Error value = ResourceSaver.Save(res, saveFile);

        if (value != Error.Ok)
            GD.PushError("Failed to import SARC in ResourceSaver: " + value.ToString());

        return value;
    }

    public override string _GetImporterName()
    {
        return "nindot.sarc";
    }

    public override string _GetVisibleName()
    {
        return "SARC Archive";
    }

    public override string[] _GetRecognizedExtensions()
    {
        return ["szs", "sarc", "zs"];
    }

    public override string _GetSaveExtension()
    {
        return "tres";
    }

    public override string _GetResourceType()
    {
        return "SarcResource";
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

#else

// If this project is being exported as a release build, lacking this class definition can cause
// GDScript parsing errors. This empty node-inherited version of the class solves this problem
// in a very janky and weird way :)

using Godot;

namespace Nindot
{
    [GlobalClass]
    public partial class SarcResourceImport : Node {}
}

#endif