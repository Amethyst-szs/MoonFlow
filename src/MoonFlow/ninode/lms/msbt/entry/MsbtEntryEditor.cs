using Godot;
using System;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;

namespace MoonFlow.LMS.Msbt;

public partial class MsbtEntryEditor(MsbpFile project, MsbtEntry entry) : VBoxContainer
{
	private readonly MsbpFile Project = project;
	private readonly MsbtEntry Entry = entry;

	public override void _Ready()
	{
		var pageHolder = GD.Load<PackedScene>("res://ninode/lms/msbt/entry/components/msbt_entry_page_holder.tscn");
		var pageSeparator = GD.Load<PackedScene>("res://ninode/lms/msbt/entry/components/msbt_entry_page_separator.tscn");

		BuildSeparator(pageSeparator, -1);

		for (int i = 0; i < Entry.Pages.Count; i++)
		{
			var page = Entry.Pages[i];

			var holder = (MsbtEntryPageHolder)pageHolder.Instantiate();

			holder.Connect(MsbtEntryPageHolder.SignalName.PageDelete,
				Callable.From(new Action<MsbtPageEditor>(OnDeletePage)));

			holder.Connect(MsbtEntryPageHolder.SignalName.PageOrganize,
				Callable.From(new Action<MsbtPageEditor, int>(OnOrganizePage)));

			AddChild(holder.Init(Project, page));
			BuildSeparator(pageSeparator, i);
		}

		UpdateOrganizationButtons();
	}

	// ====================================================== //
	// ==================== Signal Events =================== //
	// ====================================================== //

	public void OnDeletePage(MsbtPageEditor page)
	{
		Entry.Pages.Remove(page.Page);
		
		foreach (var child in GetChildren())
			child.QueueFree();

		GetTree().CreateTimer(0).Timeout += _Ready;
	}

	public void OnOrganizePage(MsbtPageEditor page, int offset)
	{
		int index = Entry.Pages.IndexOf(page.Page);

		Entry.Pages.Remove(page.Page);
		Entry.Pages.Insert(index + offset, page.Page);

		foreach (var child in GetChildren())
			child.QueueFree();

		GetTree().CreateTimer(0).Timeout += _Ready;
	}

	public void OnAddPage(int index)
	{
		Entry.Pages.Insert(index + 1, []);

		foreach (var child in GetChildren())
			child.QueueFree();

		GetTree().CreateTimer(0).Timeout += _Ready;
	}

	// ====================================================== //
	// ====================== Utilities ===================== //
	// ====================================================== //

	private void BuildSeparator(PackedScene scene, int index)
	{
		var sep = (MsbtEntryPageSeparator)scene.Instantiate();
		sep.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		sep.PageIndex = index;

		sep.Connect(MsbtEntryPageSeparator.SignalName.AddPage, Callable.From(new Action<int>(OnAddPage)));
		AddChild(sep);
	}

	private void UpdateOrganizationButtons()
	{
		foreach (var node in GetChildren())
		{
			if (node.GetType() != typeof(MsbtEntryPageHolder))
				continue;

			((MsbtEntryPageHolder)node).SetupButtonActiveness();
		}
	}
}
