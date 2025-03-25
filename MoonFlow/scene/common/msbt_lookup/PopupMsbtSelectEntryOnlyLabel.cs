using MoonFlow.Project.Cache;
using System.Collections.Generic;

namespace MoonFlow.Scene;

[SceneUid("uid://d3ptll5plbktm")]
public partial class PopupMsbtSelectEntryOnlyLabel : PopupMsbtSelectEntry
{
	public ProjectLabelCache.ArchiveType Archive = ProjectLabelCache.ArchiveType.SYSTEM;
	public string File = "";

	#region Label Lookup

	protected override List<ProjectLabelCache.LabelLookupResult> LookupTerm(ProjectLabelCache cache, string term)
	{
		return cache.LookupLabelInFileExact(Archive, File, term);
	}

	#endregion
}
