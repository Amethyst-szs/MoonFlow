using System;
using System.IO;
using Godot;

using MoonFlow.Ext;
using MoonFlow.Project;
using MoonFlow.Scene.EditorEvent;

namespace MoonFlow.Scene.Home;

public partial class TabEventFileAccessor : Node
{
    private TabEvent Parent = null;

    public override void _Ready()
    {
        Parent = this.FindParentByType<TabEvent>();
    }

    private void OnDeleteFile()
    {
        var arc = Parent.SelectedArchive;
        if (arc == null)
        {
            GD.PushError("No archive selected!");
            return;
        }

        var @event = Parent.SelectedEvent;

        // If an archive is selected without a specific event, delete archive and reload
        if (@event == null)
        {
            ProjectManager.GetProject().EventArcHolder.TryDeleteArchive(arc);
            Parent.GenerateFileList();
            return;
        }

        // If a specific event is selected, delete that byml from archive
        if (!arc.Content.ContainsKey(@event))
            throw new Exception("Selected event isn't present in selected archive!");
        
        var hashPath = GraphMetaHolder.GetPath(arc.Name, @event);
        if (File.Exists(hashPath))
            File.Delete(hashPath);
        
        arc.Content.Remove(@event);
        arc.WriteArchive();

        Parent.GenerateFileList();
    }
}
