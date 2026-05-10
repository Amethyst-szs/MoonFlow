using Godot;
using System;

namespace MoonFlow.Scene.Home;

public partial class FileListSortMenu : MenuButton
{
    public enum SortMode : int
	{
		Default = 0,
		Alphabet = 1,
        AlphabetReverse = 2,
        LastModified = 3,
        LastModifiedReverse = 4,
        Size = 5,
        SizeReverse = 6,
	}

    [Signal]
    public delegate void SortMethodChangedEventHandler(SortMode mode);

    public override void _Ready()
    {
        GetPopup().Connect(PopupMenu.SignalName.IndexPressed, Callable.From(new Action<int>(OnPopupIndexPressed)));
    }

    private void OnPopupIndexPressed(int idx)
    {
        SetCurrentSortModeInMenu(idx);
        SortMode mode = (SortMode)idx;
        EmitSignalSortMethodChanged(mode);
    }

    public void SetCurrentSortModeInMenu(int idx)
    {
        PopupMenu menu = GetPopup();
        for (int i = 0; i < menu.ItemCount; i++)
            menu.SetItemChecked(i, idx == i);
    }
}
