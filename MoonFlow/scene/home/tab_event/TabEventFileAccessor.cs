using Godot;
using MoonFlow.Project;

namespace MoonFlow.Scene.Home;

public partial class TabEventFileAccessor : TabFileAccessorBase
{
    private TabEvent Parent = null;

    private EventDataArchive CopySourceArchive = null;
    private string CopySourceEvent = null;

    public override void _Ready()
    {
        Parent = this.FindParentByType<TabEvent>();
        UpdateCopyCutPasteButton();
    }

    #region Signals

    public void OnArchiveSelected(EventDataArchive arc, string @event)
    {
        var isProj = arc.Source == EventDataArchive.ArchiveSource.PROJECT;
        DeleteButton.Disabled = !isProj && @event == null;

        UpdateCopyCutPasteButton();
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
        UpdateCopyCutPasteButton();
    }
    protected override void OnCutFile()
    {
        if (Parent.SelectedEvent == null)
            return;

        base.OnCutFile();

        CopySourceArchive = Parent.SelectedArchive;
        CopySourceEvent = Parent.SelectedEvent;
        UpdateCopyCutPasteButton();
    }
    private void OnPasteFile()
    {
        if (CopySourceArchive == null)
            return;

        if (CopySourceEvent == null)
            OnPasteArchive();
        else
            OnPasteEvent();

        if (IsCut)
            ClearCopyContents();
    }

    private void OnPasteArchive()
    {
        var arcHolder = ProjectManager.GetProject().EventArcHolder;

        string nameBase = CopySourceArchive.Name.RemoveFileExtension();
        string name = nameBase + ".szs";
        int nameIdx = 2;

        while (arcHolder.Content.ContainsKey(name))
        {
            name = nameBase + nameIdx + ".szs";
            nameIdx++;
        }

        arcHolder.TryDuplicateArchive(CopySourceArchive, name, ProjectManager.GetPath());
        Parent.GenerateFileList();
    }
    private void OnPasteEvent()
    {
        var path = ProjectManager.GetPath();
        var target = Parent.SelectedArchive;

        string nameBase = CopySourceEvent.RemoveFileExtension();
        string name = nameBase + ".byml";
        int nameIdx = 2;

        while (target.Content.ContainsKey(name))
        {
            name = nameBase + nameIdx + ".byml";
            nameIdx++;
        }

        ProjectEventDataArchiveHolder.TryDuplicateGraph(CopySourceArchive, target, CopySourceEvent, name, path);

        if (IsCut)
            ProjectEventDataArchiveHolder.DeleteGraph(CopySourceArchive, CopySourceEvent, path);

        Parent.GenerateFileList();
    }

    private void OnDuplicateArchive(string newName)
    {
        if (!IsArchiveNameUnique(ref newName, out ProjectEventDataArchiveHolder arcHolder))
            return;

        arcHolder.TryDuplicateArchive(Parent.SelectedArchive, newName, ProjectManager.GetPath());
        Parent.GenerateFileList();
    }
    private void OnDuplicateEvent(string newName)
    {
        if (!IsEventNameUnique(ref newName))
            return;

        var source = Parent.SelectedEvent;
        ProjectEventDataArchiveHolder.TryDuplicateGraph(Parent.SelectedArchive, source, newName, ProjectManager.GetPath());
        Parent.GenerateFileList();
    }

    private void OnNewArchiveFooterPressed()
    {
        var eventPopup = GetNode<Popup>("Popup_NewArchive");
        eventPopup.PopupCentered();
        eventPopup.Call("init_data", "");
    }
    private void OnNewEventFooterPressed()
    {
        var eventPopup = GetNode<Popup>("Popup_NewEvent");
        eventPopup.PopupCentered();
        eventPopup.Call("init_data", "");
    }

    private void OnNewArchive(string newName)
    {
        if (!IsArchiveNameUnique(ref newName, out ProjectEventDataArchiveHolder arcHolder))
            return;

        arcHolder.TryNewArchive(newName);
        Parent.GenerateFileList();
    }
    private void OnNewEvent(string newName)
    {
        if (!IsEventNameUnique(ref newName, false))
            return;

        ProjectEventDataArchiveHolder.NewGraph(Parent.SelectedArchive, newName);
        Parent.GenerateFileList();
    }

    private void OnRenameArchive(string newName)
    {
        if (!IsArchiveNameUnique(ref newName, out ProjectEventDataArchiveHolder arcHolder))
            return;

        var select = Parent.SelectedArchive;
        var path = ProjectManager.GetPath();

        arcHolder.TryDuplicateArchive(select, newName, path);
        arcHolder.TryDeleteArchive(select, path);

        Parent.GenerateFileList();
    }
    private void OnRenameEvent(string newName)
    {
        if (!IsEventNameUnique(ref newName))
            return;

        var select = Parent.SelectedArchive;
        var source = Parent.SelectedEvent;
        var path = ProjectManager.GetPath();

        ProjectEventDataArchiveHolder.TryDuplicateGraph(select, source, newName, path);
        ProjectEventDataArchiveHolder.DeleteGraph(select, source, path);

        Parent.GenerateFileList();
    }

    private void OnDeleteFile()
    {
        var path = ProjectManager.GetPath();
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
            ProjectManager.GetProject().EventArcHolder.TryDeleteArchive(arc, path);
        else
            ProjectEventDataArchiveHolder.DeleteGraph(arc, @event, path);

        Parent.GenerateFileList();
    }

    #endregion

    #region Utility

    private void ClearCopyContents()
    {
        IsCut = false;
        CopySourceArchive = null;
        CopySourceEvent = null;
        UpdateCopyCutPasteButton();
    }
    private void UpdateCopyCutPasteButton()
    {
        CutButton.Disabled = Parent.SelectedEvent == null;
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

    private bool IsEventNameUnique(ref string newName, bool isRequireEventSelection = true)
    {
        if (!newName.EndsWith(".bmyl")) newName += ".byml";

        if (Parent.SelectedArchive == null || (Parent.SelectedEvent == null && isRequireEventSelection))
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
