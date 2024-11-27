using Godot;
using System;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;
using System.Linq;
using MoonFlow.Fs.Rom;
using Nindot.Al.SMO;

namespace MoonFlow.LMS.Msbt;

public partial class MsbtEditor : PanelContainer
{
	private string EntrySearchString = "";

	private void OnSearchEntryListUpdated(string match)
	{
		EntrySearchString = match;

		// Update entry count in other components
		int entryCount = File.GetEntryCount();

		// Update visiblity of selectors
		if (match == string.Empty)
		{
			foreach (var child in EntryList.GetChildren())
				((Control)child).Show();

			EmitSignal(SignalName.EntryCountUpdated, [entryCount, entryCount]);
			return;
		}

		int matching = 0;
		Node firstMatch = null;
		bool isSearchForNewSelection = true;

		foreach (var child in EntryList.GetChildren())
		{
			if (child.GetType() != typeof(Button)) continue;

			var isMatch = child.Name.ToString().Contains(match, StringComparison.OrdinalIgnoreCase);
			((Button)child).Visible = isMatch;
			matching += isMatch ? 1 : 0;

			if (firstMatch == null && isMatch)
				firstMatch = child;

			if (!isMatch && (EntryListSelection == child || isSearchForNewSelection))
			{
				if (EntryList.GetChildCount() == 0)
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

		EmitSignal(SignalName.EntryCountUpdated, [entryCount, matching]);
	}
}
