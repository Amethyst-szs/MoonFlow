using Godot;
using System;

using Nindot.LMS.Msbt;
using MoonFlow.Project.Database;
using MoonFlow.Project;
using MoonFlow.Project.Templates;

namespace MoonFlow.Scene.Home;

public partial class TabMsbtFileAccessor : TabFileAccessorBase
{
	private TabMsbt Parent = null;
	private SarcMsbtFile CopyContent = null;

	public override void _Ready()
	{
		Parent = this.FindParentByType<TabMsbt>();
	}

	#region Signals

	public void OnFileSelected(SarcMsbtFile selection)
	{
		var isCopyPaste = selection.Sarc.Name != "StageMessage.szs";
		CopyButton.Disabled = !isCopyPaste;
		CutButton.Disabled = !isCopyPaste;

		if (IsCut)
			PasteButton.Disabled = !isCopyPaste || CopyContent == null || CopyContent.Sarc == selection.Sarc;
		else
			PasteButton.Disabled = !isCopyPaste || CopyContent == null;

		// Check if file is deletable
		var msbpDB = ProjectManager.GetMSBP().Project.Content;
		var selectTarget = selection.Name.TrimSuffix(".msbt") + ".mstxt";

		var result = msbpDB.Find(s => s.EndsWith(selectTarget));
		DeleteButton.Disabled = result.Contains("/Reference/");
	}

	protected override void OnCopyFile()
	{
		base.OnCopyFile();

		CopyContent = Parent.SelectedFile;
		OnFileSelected(CopyContent);
	}
	protected override void OnCutFile()
	{
		base.OnCutFile();

		CopyContent = Parent.SelectedFile;
		OnFileSelected(CopyContent);
	}

	private void OnPasteFile()
	{
		if (CopyContent == null)
			return;

		var arcHolder = ProjectManager.GetMSBT();
		if (arcHolder == null)
			return;

		// Create file name
		var nameBase = CopyContent.Name.RemoveFileExtension();
		var name = nameBase + ".msbt";
		var nameResolverIdx = 2;

		while (Parent.SelectedFile.Sarc.Content.ContainsKey(name))
		{
			name = nameBase + nameResolverIdx + ".msbt";
			nameResolverIdx++;
		}

		// Paste file in all languages
		foreach (var lang in arcHolder)
		{
			// Lookup source and target archives for this language
			var localSourceArc = lang.Value.GetArchiveByFileName(CopyContent.Sarc.Name)
			?? throw new NullReferenceException("Could not resolve source archive!");

			var localTargetArc = lang.Value.GetArchiveByFileName(Parent.SelectedFile.Sarc.Name)
			?? throw new NullReferenceException("Could not resolve target archive!");

			// Ensure the language contains a text file with the copy source's name
			if (!localSourceArc.Content.TryGetValue(CopyContent.Name, out ArraySegment<byte> data))
			{
				GD.PushWarning("Skipping paste for " + lang.Key + " due to lack of source file");
				continue;
			}

			lang.Value.Metadata.EntryDuplicate(CopyContent.Sarc, CopyContent.Name, localTargetArc.Name, name);

			localTargetArc.Content.Add(name, data.ToArray());
			localTargetArc.WriteArchive();
		}

		if (!IsCut)
		{
			ProjectManager.UpdateMsbpProjectSources();
			Parent.ReloadInterface(true);
			return;
		}

		OnDeleteFile(CopyContent.Sarc.Name, CopyContent.Name);
		CopyContent = null;
		IsCut = false;
	}

	private void OnDuplicateFile(string arcName, string newName)
	{
		if (newName == string.Empty)
			return;

		if (!TryGetWorld(arcName, newName, out WorldInfo world))
			return;

		var target = TabMsbt.GetFileName(newName);
		var arcHolder = ProjectManager.GetMSBT();

		var sourceArc = Parent.SelectedFile.Sarc;
		if (!IsFileNameValid(target, sourceArc))
			return;

		// Duplicate file in all languages
		foreach (var lang in arcHolder)
		{
			// Lookup source and target archives for this language
			var localSourceArc = lang.Value.GetArchiveByFileName(sourceArc.Name);
			if (localSourceArc == null)
				throw new NullReferenceException("Could not resolve source archive!");

			var localTargetArc = lang.Value.GetArchiveByFileName(arcName);
			if (localTargetArc == null)
				throw new NullReferenceException("Could not resolve source and/or target archive!");

			var sourceFile = Parent.SelectedFile.Name;
			if (!localSourceArc.Content.TryGetValue(sourceFile, out ArraySegment<byte> data))
			{
				GD.PushWarning("Skipping duplication for " + lang.Key + " due to lack of source file");
				continue;
			}

			lang.Value.Metadata.EntryDuplicate(localSourceArc, sourceFile, localTargetArc.Name, target);

			localTargetArc.Content.Add(target, data.ToArray());
			localTargetArc.WriteArchive();
		}

		ProjectManager.UpdateMsbpProjectSources();
		Parent.ReloadInterface(true);
	}

	private void OnNewFile(string arcName, string newName)
	{
		if (newName == string.Empty)
			return;

		if (!TryGetWorld(arcName, newName, out WorldInfo world))
			return;

		var target = TabMsbt.GetFileName(newName);
		var arcHolder = ProjectManager.GetMSBT();

		var sourceArc = Parent.SelectedFile.Sarc;
		if (!IsFileNameValid(target, sourceArc))
			return;

		// Create file in all languages
		foreach (var lang in arcHolder)
		{
			var targetArc = lang.Value.GetArchiveByFileName(arcName);
			if (targetArc == null)
				throw new NullReferenceException("Could not resolve source and/or target archive!");

			targetArc.Content.Add(target, LmsTemplates.EmptyMsbt);
			targetArc.WriteArchive();
		}

		ProjectManager.UpdateMsbpProjectSources();
		Parent.ReloadInterface(true);
	}

	private void OnDeleteFile()
	{
		var arcName = Parent.SelectedFile.Sarc.Name;
		var fileName = Parent.SelectedFile.Name;
		OnDeleteFile(arcName, fileName);
	}
	private void OnDeleteFile(string arcName, string fileName)
	{
		// Remove from all archives
		var arcHolder = ProjectManager.GetMSBT();
		foreach (var lang in arcHolder)
		{
			var targetArc = lang.Value.GetArchiveByFileName(arcName);
			if (targetArc == null)
				throw new NullReferenceException("Could not resolve source and/or target archive!");

			lang.Value.Metadata.EntryRemove(targetArc, fileName);

			targetArc.Content.Remove(fileName);
			targetArc.WriteArchive();
		}

		ProjectManager.UpdateMsbpProjectSources();
		Parent.ReloadInterface(true);
	}

	private void OnRenameFile(string archive, string file)
	{
		if (file == string.Empty)
			return;

		if (!TryGetWorld(archive, file, out _))
			return;

		var sourceArc = Parent.SelectedFile.Sarc;
		if (!IsFileNameValid(TabMsbt.GetFileName(file), sourceArc))
			return;

		var sourceArcName = sourceArc.Name;
		var sourceFileName = Parent.SelectedFile.Name;

		OnDuplicateFile(archive, file);
		OnDeleteFile(sourceArcName, sourceFileName);
	}

	#endregion

	#region Utility

	private bool TryGetWorld(string arc, string targetName, out WorldInfo world)
	{
		if (arc != "StageMessage.szs")
		{
			world = null;
			return true;
		}

		var db = ProjectManager.GetDB();
		world = db.GetWorldInfoByStageName(targetName);

		if (world != null)
			return true;

		GetNode<AcceptDialog>("Dialog_CreateError_WorldList").Popup();
		return false;
	}

	#endregion
}
