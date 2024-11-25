using System;
using Godot;

using MoonFlow.LMS.Msbt;

public class MsbtContextMenu
{
	enum MenuItems : int
	{
		UNDO = (int)TextEdit.MenuItems.Max + 1, // Start our enum after the end of the TextEdit enum
		REDO,

		CUT,
		COPY,
		PASTE,
		DELETE,

		SELECT_ALL,
	}

	private readonly MsbtPageEditor Editor = null;
	private readonly PopupMenu Menu = null;

	public MsbtContextMenu(MsbtPageEditor editor)
	{
		throw new NotImplementedException();
		// Setup global variables
		Editor = editor;
		Menu = editor.GetMenu();

		// Destroy current contents of menu
		Menu.ItemCount = 0;

		// Create all items in menu
		Menu.AddItem("Undo", (int)MenuItems.UNDO);
		Menu.AddItem("Redo", (int)MenuItems.REDO);
		Menu.AddSeparator("");
		Menu.AddItem("Cut", (int)MenuItems.CUT);
		Menu.AddItem("Copy", (int)MenuItems.COPY);
		Menu.AddItem("Paste", (int)MenuItems.PASTE);
		Menu.AddItem("Delete", (int)MenuItems.DELETE);
		Menu.AddSeparator("");
		Menu.AddItem("Select All", (int)MenuItems.SELECT_ALL);

		// Connect signals for menu functionality
		Menu.IdPressed += IdPressed;
	}

	private void IdPressed(long id)
	{
		var idItem = (MenuItems)id;

		(idItem switch
		{
			MenuItems.UNDO => (Action)Editor.Undo,
			MenuItems.REDO => Editor.Redo,
			MenuItems.CUT => Editor.CutViaContextMenu,
			MenuItems.COPY => Editor.CopyViaContextMenu,
			MenuItems.PASTE => Editor.PasteViaContextMenu,
			MenuItems.DELETE => Editor.DeleteViaContextMenu,
			MenuItems.SELECT_ALL => Editor.SelectAll,
			_ => throw new ArgumentOutOfRangeException(nameof(id))
		})();
	}
}
