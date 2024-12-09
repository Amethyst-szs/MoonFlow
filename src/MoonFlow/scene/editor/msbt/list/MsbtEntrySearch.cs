using Godot;
using System;

namespace MoonFlow.Scene.EditorMsbt;

public partial class MsbtEntryList : VBoxContainer
{
	private string EntrySearchString = "";

	public void UpdateEntryCount()
	{
		OnSearchEntryListUpdated(EntrySearchString);
	}

	public void UpdateSearch(string searchStr)
	{
		EntrySearchString = searchStr;
		OnSearchEntryListUpdated(EntrySearchString);
	}

	private void OnSearchEntryListUpdated(string match)
	{
		// Update entry count in other components
		int entryCount = Parent.File.GetEntryCount();

		// Update visiblity of selectors
		if (match == string.Empty)
		{
			foreach (var child in GetChildren())
				((Control)child).Show();

			UpdateEntryCountLabel(entryCount, entryCount);
			return;
		}

		int matching = 0;
		Node firstMatch = null;
		bool isSearchForNewSelection = true;

		foreach (var child in GetChildren())
		{
			if (child.GetType() != typeof(Button)) continue;

			var isMatch = child.Name.ToString().Contains(match, StringComparison.OrdinalIgnoreCase);
			((Button)child).Visible = isMatch;
			matching += isMatch ? 1 : 0;

			if (firstMatch == null && isMatch)
				firstMatch = child;

			if (!isMatch && (EntryListSelection == child || isSearchForNewSelection))
			{
				if (GetChildCount() == 0)
				{
					OnEntrySelected("", false);
					continue;
				}

				if (isSearchForNewSelection && firstMatch != null)
				{
					isSearchForNewSelection = false;
					OnEntrySelected(firstMatch.Name, false);
					continue;
				}
			}
		}

		UpdateEntryCountLabel(entryCount, matching);
	}

	private void UpdateEntryCountLabel(int total) { UpdateEntryCountLabel(total, total); }

	private void UpdateEntryCountLabel(int total, int matching)
	{
		if (total == matching)
		{
			EntryCount.Text = total.ToString() + " Entries";
			return;
		}

		EntryCount.Text = string.Format("{0}/{1} Entries", matching, total);
	}
}
