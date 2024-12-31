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

    private void OnCommonFooterPressed(string actionName)
    {
        if (Parent.SelectedArchive == null)
            return;
        
        if (Parent.SelectedEvent == null)
        {
            var archivePopup = GetNode<Popup>("Popup_" + actionName + "Archive");
            archivePopup.PopupCentered();
            archivePopup.Call("init_data", Parent.SelectedArchive.Name);
            return;
        }

        var eventPopup = GetNode<Popup>("Popup_" + actionName + "Event");
        eventPopup.PopupCentered();
        eventPopup.Call("init_data", Parent.SelectedEvent);
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

    private void OnDuplicateArchive(string newName)
    {
        if (!IsArchiveNameUnique(ref newName, out ProjectEventDataArchiveHolder arcHolder))
            return;

        arcHolder.TryDuplicateArchive(Parent.SelectedArchive, newName);
        Parent.GenerateFileList();
    }
    private void OnDuplicateEvent(string newName)
    {
        if (!IsEventNameUnique(ref newName))
            return;

        var source = Parent.SelectedEvent;
        ProjectEventDataArchiveHolder.TryDuplicateGraph(Parent.SelectedArchive, source, newName);
        Parent.GenerateFileList();
    }

    private void OnNewArchiveFooterPressed()
    {
        var eventPopup = GetNode<Popup>("Popup_NewArchive");
        eventPopup.PopupCentered();
        eventPopup.Call("init_data", Parent.SelectedEvent);
    }
    private void OnNewArchive(string newName)
    {
        if (!IsArchiveNameUnique(ref newName, out ProjectEventDataArchiveHolder arcHolder))
            return;

        arcHolder.TryNewArchive(newName);
        Parent.GenerateFileList();
    }

    private void OnRenameArchive(string newName)
    {
        if (!IsArchiveNameUnique(ref newName, out ProjectEventDataArchiveHolder arcHolder))
            return;

        var select = Parent.SelectedArchive;
        arcHolder.TryDuplicateArchive(select, newName);
        arcHolder.TryDeleteArchive(select);

        Parent.GenerateFileList();
    }
    private void OnRenameEvent(string newName)
    {
        if (!IsEventNameUnique(ref newName))
            return;
        
        var select = Parent.SelectedArchive;
        var source = Parent.SelectedEvent;

        ProjectEventDataArchiveHolder.TryDuplicateGraph(select, source, newName);
        ProjectEventDataArchiveHolder.DeleteGraph(select, source);

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

    private bool IsArchiveNameUnique(ref string newName, out ProjectEventDataArchiveHolder arcHolder)
    {
        if (!newName.EndsWith(".szs")) newName += ".szs";

        arcHolder = ProjectManager.GetProject().EventArcHolder;
        if (arcHolder.Content.ContainsKey(newName))
        {
            ThrowDuplicateNameDialog();
            return false;
        }

        return true;
    }

    private bool IsEventNameUnique(ref string newName)
    {
        if (!newName.EndsWith(".bmyl")) newName += ".byml";

        if (Parent.SelectedArchive == null || Parent.SelectedEvent == null)
            return false;
        
        if (Parent.SelectedArchive.Content.ContainsKey(newName))
        {
            ThrowDuplicateNameDialog();
            return false;
        }

        return true;
    }

    #endregion
}
