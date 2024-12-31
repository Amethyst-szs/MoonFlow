using System;
using System.IO;
using Godot;

using Nindot;

using MoonFlow.Ext;
using MoonFlow.Project;
using MoonFlow.Scene.EditorEvent;

namespace MoonFlow.Scene.Home;

public partial class TabEventFileAccessor : TabFileAccessorBase
{
    private TabEvent Parent = null;

    private EventDataArchive CopySourceArchive = null;
    private string CopySourceEvent = null;

    public override void _Ready()
    {
        Parent = this.FindParentByType<TabEvent>();
        UpdatePasteButton();
    }

    #region Signals

    public void OnArchiveSelected(EventDataArchive arc, string @event)
    {
        var isProj = arc.Source == EventDataArchive.ArchiveSource.PROJECT;
        DeleteButton.Disabled = !isProj && @event == null;
    }

    protected override void OnCopyFile()
    {
        base.OnCopyFile();

        CopySourceArchive = Parent.SelectedArchive;
        CopySourceEvent = Parent.SelectedEvent;
        UpdatePasteButton();
    }
    protected override void OnCutFile()
    {
        base.OnCutFile();

        CopySourceArchive = Parent.SelectedArchive;
        CopySourceEvent = Parent.SelectedEvent;
        UpdatePasteButton();
    }

    private void OnPasteFile()
    {
        if (CopySourceArchive == null || CopySourceEvent == null)
            return;
        
        GD.Print("PLACEHOLDER FUNCTION");
    }

    private void OnDuplicateFooterPressed()
    {
        if (Parent.SelectedArchive == null)
            return;
        
        if (Parent.SelectedEvent == null)
        {
            var archivePopup = GetNode<Popup>("Popup_DuplicateArchive");
            archivePopup.PopupCentered();
            archivePopup.Call("init_data", Parent.SelectedArchive.Name);
            return;
        }

        var eventPopup = GetNode<Popup>("Popup_DuplicateEvent");
        eventPopup.PopupCentered();
        eventPopup.Call("init_data", Parent.SelectedEvent);
    }
    private void OnDuplicateArchive(string newName)
    {
        if (!newName.EndsWith(".szs")) newName += ".szs";

        var arcHolder = ProjectManager.GetProject().EventArcHolder;
        if (arcHolder.Content.ContainsKey(newName))
        {
            ThrowDuplicateNameDialog();
            return;
        }

        arcHolder.TryDuplicateArchive(Parent.SelectedArchive, newName);
        Parent.GenerateFileList();
    }
    private void OnDuplicateEvent(string newName)
    {
        if (!newName.EndsWith(".bmyl")) newName += ".byml";

        if (Parent.SelectedArchive == null || Parent.SelectedEvent == null)
            return;
        
        if (Parent.SelectedArchive.Content.ContainsKey(newName))
        {
            ThrowDuplicateNameDialog();
            return;
        }

        var source = Parent.SelectedEvent;
        ProjectEventDataArchiveHolder.TryDuplicateGraph(Parent.SelectedArchive, source, newName);
        Parent.GenerateFileList();
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

        if (CopySourceArchive == arc)
            ClearCopyContents();

        if (@event == null)
            ProjectManager.GetProject().EventArcHolder.TryDeleteArchive(arc);
        else
            ProjectEventDataArchiveHolder.DeleteGraph(arc, @event);

        Parent.GenerateFileList();
    }

    #endregion

    #region Utility

    private void ClearCopyContents()
    {
        CopySourceArchive = null;
        CopySourceEvent = null;
        UpdatePasteButton();
    }
    private void UpdatePasteButton()
    {
        PasteButton.Disabled = CopySourceArchive == null;
    }

    #endregion
}
