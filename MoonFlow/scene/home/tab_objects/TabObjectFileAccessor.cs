using Godot;
using MoonFlow.Project;
using Nindot;
using System;

namespace MoonFlow.Scene.Home;

public partial class TabObjectFileAccessor : TabFileAccessorBase
{
    private TabObjects Parent = null;

    private string CopySource = null;

    public override void _Ready()
    {
        Parent = this.FindParentByType<TabObjects>();
    }

    #region Signals

    private void OnNewArchiveSubmenuFooterPressed() { throw new NotImplementedException(); }
    private void OnCommonFooterPressed(string actionName)
    {
        // if (Parent.SelectedArc == null)
        //     return;

        // var archivePopup = GetNode<Popup>("Popup_" + actionName);
        // archivePopup.PopupCentered();
        // archivePopup.Call("init_data", Parent.SelectedArc);
        return;
    }

    private void OnNewArchive(string newName) { throw new NotImplementedException(); }
    private void OnDuplicateArchive(string newName) { throw new NotImplementedException(); }
    private void OnRenameArchive(string newName) { throw new NotImplementedException(); }
    private void OnDeleteArchive() { throw new NotImplementedException(); }

    #endregion
}

