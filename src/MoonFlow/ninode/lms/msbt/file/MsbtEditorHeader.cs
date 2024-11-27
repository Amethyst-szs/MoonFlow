using Godot;
using System;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;
using System.Linq;

namespace MoonFlow.LMS.Msbt;

public partial class MsbtEditor : PanelContainer
{
    private enum FileButtons
    {
        SAVE
    };

    private void InitHeader()
    {
        var file = GetNode<PopupMenu>("%File");
        file.IdPressed += OnHeaderFileOptionPressed;

        InitHeaderItem(file, (int)FileButtons.SAVE, "Save", "ui_save");
    }

    private void OnHeaderFileOptionPressed(long id)
    {
        (id switch
		{
			(long)FileButtons.SAVE => (Action)SaveFile,
			_ => throw new ArgumentOutOfRangeException(nameof(id))
		})();
    }

    // ====================================================== //
    // =============== Initilization Utilities ============== //
    // ====================================================== //

    private static void InitHeaderItem(PopupMenu menu, int id, string name)
    {
        menu.AddItem(name, id);
    }
    private static void InitHeaderItem(PopupMenu menu, int id, string name, string inputAction)
    {
        var shortcut = new Shortcut();
        var action = new InputEventAction { Action = inputAction };
        shortcut.Events.Add(action);
        menu.AddItem(name, id);
        menu.SetItemShortcut(menu.GetItemIndex(id), shortcut, true);
    }
}