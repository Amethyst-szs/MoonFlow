using Godot;
using System;

using Nindot;

using MoonFlow.Ext;
using MoonFlow.Project.Database;
using MoonFlow.Project;
using MoonFlow.Project.Templates;

namespace MoonFlow.Scene.Home;

public partial class TabMsbtFileAccessor : Node
{
    private TabMsbt Parent = null;

    public override void _Ready()
    {
        Parent = this.FindParentByType<TabMsbt>();
    }

    #region Signals

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
			var targetArc = lang.Value.GetArchiveByFileName(arcName);
			if (targetArc == null)
				throw new NullReferenceException("Could not resolve source and/or target archive!");

			if (!targetArc.Content.TryGetValue(Parent.SelectedFile.Name, out ArraySegment<byte> data))
			{
				GD.PushWarning("Skipping duplication for " + lang.Key + " due to lack of source file");
				continue;
			}

			targetArc.Content.Add(target, data.ToArray());
			targetArc.WriteArchive();
		}

		PublishMsbtToProject(arcName, newName, world);
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

		PublishMsbtToProject(arcName, newName, world);
		Parent.ReloadInterface(true);
	}

	private void OnDeleteFile()
	{
		var arcName = Parent.SelectedFile.Sarc.Name;
		var fileName = Parent.SelectedFile.Name;

		// Remove from all archives
		var arcHolder = ProjectManager.GetMSBT();
		foreach (var lang in arcHolder)
		{
			var targetArc = lang.Value.GetArchiveByFileName(arcName);
			if (targetArc == null)
				throw new NullReferenceException("Could not resolve source and/or target archive!");
			
			targetArc.Content.Remove(fileName);
			targetArc.WriteArchive();
		}

		// Remove from MSBP
		ProjectManager.GetMSBPHolder().UnpublishFile(arcName, fileName);
		Parent.ReloadInterface(true);
	}

    #endregion

    #region Utility

    private bool IsFileNameValid(string name, SarcFile sourceArc)
	{
		if (sourceArc == null || sourceArc.Content.ContainsKey(name))
		{
			GetNode<AcceptDialog>("Dialog_CreateError_DuplicateName").Popup();
			return false;
		}

		return true;
	}

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

	private static void PublishMsbtToProject(string arcName, string newName, WorldInfo world)
	{
		// Publish entry to ProjectData
		var msbp = ProjectManager.GetMSBPHolder();

		if (world == null)
			msbp.PublishFile(arcName, newName);
		else
			msbp.PublishFile(arcName, newName, world);
	}

    #endregion
}
