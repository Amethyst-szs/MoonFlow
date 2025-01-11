using MoonFlow.Project.Cache;
using System.Collections.Generic;

namespace MoonFlow.Scene;

[ScenePath("res://scene/common/popup/popup_msbt_select_entry_only_label.tscn")]
public partial class PopupMsbtSelectEntryOnlyLabel : PopupMsbtSelectEntry
{
	public ProjectLabelCache.ArchiveType Archive = ProjectLabelCache.ArchiveType.SYSTEM;
	public string File = "";

	#region Label Lookup

	protected override List<ProjectLabelCache.LabelLookupResult> LookupTerm(ProjectLabelCache cache, string term)
	{
		return cache.LookupLabelInFileExact(Archive, File, term);
	}

	protected override List<ProjectLabelCache.LabelLookupResult> LookupTermInFile(ProjectLabelCache cache, string term, string file)
	{
		return cache.LookupLabelInFileExact(Archive, File, term);
	}

	#endregion
}
