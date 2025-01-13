using Godot;
using System;

using Nindot.LMS.Msbt;

using MoonFlow.Project;

namespace MoonFlow.Scene.EditorMsbt;

public partial class EntryListSimple : EntryListBase
{
    public override void CreateContent(SarcMsbtFile file, out string[] labels)
    {
        // Sort list of labels in alphabetical order
        labels = [.. file.GetEntryLabels()];
        Array.Sort(labels, string.Compare);

        foreach (var key in labels)
            CreateEntryListButton(key);
    }

    public override void CreateEntryListButton(string key, bool isSort = false)
    {
        CreateEntryListButton(key, key, this, isSort);
    }

    public override void CreateEntryListButton(string key, string label, Control container, bool isSort = false)
    {
        // Get metadata
        var metaH = ProjectManager.GetMSBTMetaHolder(Editor.CurrentLanguage);
        var metaHSource = ProjectManager.GetMSBTMetaHolder(Editor.DefaultLanguage);

        var meta = metaH.GetMetadata(Editor.File, key);
        var metaSource = metaHSource.GetMetadata(Editor.FileList[Editor.DefaultLanguage], key);

        // Create button
        var button = SceneCreator<EntryLabelButton>.Create();
        button.SetupButton(this, key, label, meta, metaSource);

        container.AddChild(button);

        if (!isSort)
            return;

        int moveIndex = 0;
        for (int i = 0; i < GetChildCount(); i++)
        {
            int result = string.Compare(key, GetChild(i).Name);
            if (result > 0)
            {
                moveIndex += 1;
                continue;
            }

            break;
        }

        MoveChild(button, moveIndex);
    }
}
