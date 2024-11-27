using Godot;
using System;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.LMS.Msbt;

public partial class MsbtEntryEditor(MsbpFile project, MsbtEntry entry) : VBoxContainer
{
	private readonly MsbpFile Project = project;
	private readonly MsbtEntry Entry = entry;

    public override void _Ready()
	{
		foreach (var page in Entry.Pages)
		{
			var pageEditor = new MsbtEntryPageHolder(Project, page);
			AddChild(pageEditor);
			AddChild(new HSeparator());
		}
	}
}
