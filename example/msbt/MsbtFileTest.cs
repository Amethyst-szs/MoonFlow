using Godot;
using System;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;
using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt.TagLib;

public partial class MsbtFileTest : Control
{
	public MsbpFile Project;
	public MsbtFile File;

	public override void _Ready()
	{
		// Setup project
		Project = MsbpFile.FromFilePath("res://test/lms/msbp/ProjectData-SMO.msbp");

		// Setup file
		var factory = new MsbtElementFactoryProjectSmo();
		File = MsbtFile.FromFilePath("res://test/lms/msbt/SmoUnitTesting.msbt", factory);

		// Create list of keys in file
		var keyContainer = GetNode<VBoxContainer>("Split/FileKeys/Content");
		foreach (var key in File.Content.Keys)
		{
			var button = new Button();
			button.Text = key;
			button.Pressed += () => OpenEntry(key);
			keyContainer.AddChild(button);
		}
	}

	private void OpenEntry(string entryName)
	{
		// Empty out current content box
		var content = GetNode<VBoxContainer>("Split/Content");
		foreach (var child in content.GetChildren())
			content.RemoveChild(child);

		// Get access to requested entry
		File.Content.TryGetValue(entryName, out MsbtEntry entry);
		if (entry == null) return;

		// Build content box
		RichTextLabel label = new()
		{
			FitContent = true,
			BbcodeEnabled = true,
		};

		foreach (var element in entry.Elements)
		{
			if (element.IsTag())
			{
				var elementT = (MsbtTagElement)element;

				label.PushItalics();
				label.PushColor(new Color(1.0F, 0.7F, 0.7F, 1.0F));
				label.PushFontSize(14);

				string group = Project.TagGroup_Get(elementT.GetGroupName()).Name;
				string name = elementT.GetTagNameInProject(Project);

				label.AppendText(group + " - " + name);

				label.PopAll();
				continue;
			}

			label.AppendText(element.GetText());
			continue;
		}

		content.AddChild(label);
	}
}
