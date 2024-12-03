using Godot;
using System;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;

using MoonFlow.Project;

namespace MoonFlow.LMS.Msbt;

public partial class MsbtEntryEditor(MsbtEditor parent, MsbtEntry entry, ProjectLanguageMetaHolder.Meta meta) : VBoxContainer
{
	private readonly MsbtEditor Parent = parent;
	private readonly MsbtEntry Entry = entry;
	private ProjectLanguageMetaHolder.Meta Metadata = meta;

	public override void _Ready()
	{
		// If the current language doesn't match the default language, create translation config
		bool isDefaultLang = Parent.CurrentLanguage == Parent.DefaultLanguage;
		if (!isDefaultLang)
		{
			var langConfig = SceneCreator<MsbtEntryTranslationConfig>.Create();
			langConfig.SetupNode(Metadata);

			langConfig.Connect(MsbtEntryTranslationConfig.SignalName.SyncToggled,
				Callable.From(new Action<bool>(OnSyncToggled)));

			AddChild(langConfig);
		}

		// Setup pages and page separators
		BuildSeparator(-1);
		for (int i = 0; i < Entry.Pages.Count; i++)
		{
			var page = Entry.Pages[i];

			var holder = SceneCreator<MsbtEntryPageHolder>.Create();

			holder.Connect(MsbtEntryPageHolder.SignalName.PageDelete,
				Callable.From(new Action<MsbtPageEditor>(OnDeletePage)));
			holder.Connect(MsbtEntryPageHolder.SignalName.PageOrganize,
				Callable.From(new Action<MsbtPageEditor, int>(OnOrganizePage)));

			AddChild(holder.Init(Parent.Project, page));

			BuildSeparator(i);
		}

		// Complete setup
		if (!isDefaultLang)
			OnSyncToggled(Metadata.IsDisableSync);
		else
			UpdatePageOrderingButtons();
	}

	// ====================================================== //
	// ==================== Signal Events =================== //
	// ====================================================== //

	private void OnDeletePage(MsbtPageEditor page)
	{
		Entry.Pages.Remove(page.Page);

		foreach (var child in GetChildren())
			child.QueueFree();

		GetTree().CreateTimer(0).Timeout += _Ready;
	}

	private void OnOrganizePage(MsbtPageEditor page, int offset)
	{
		int index = Entry.Pages.IndexOf(page.Page);

		Entry.Pages.Remove(page.Page);
		Entry.Pages.Insert(index + offset, page.Page);

		foreach (var child in GetChildren())
			child.QueueFree();

		GetTree().CreateTimer(0).Timeout += _Ready;
	}

	private void OnAddPage(int index)
	{
		Entry.Pages.Insert(index + 1, []);

		foreach (var child in GetChildren())
			child.QueueFree();

		GetTree().CreateTimer(0).Timeout += _Ready;
	}

	private void OnSyncToggled(bool isDisableSync)
	{
		Metadata.IsDisableSync = isDisableSync;

		foreach (var child in GetChildren())
		{
			switch (child)
			{
				case MsbtEntryPageHolder:
					((MsbtEntryPageHolder)child).HandleSyncToggled(isDisableSync);
					continue;
				case MsbtEntryPageSeparator:
					((MsbtEntryPageSeparator)child).HandleSyncToggled(isDisableSync);
					continue;
			}
		}
	}

	// ====================================================== //
	// ====================== Utilities ===================== //
	// ====================================================== //

	private void BuildSeparator(int index)
	{
		var sep = SceneCreator<MsbtEntryPageSeparator>.Create();
		sep.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		sep.PageIndex = index;

		sep.Connect(MsbtEntryPageSeparator.SignalName.AddPage, Callable.From(new Action<int>(OnAddPage)));
		AddChild(sep);
	}

	private void UpdatePageOrderingButtons()
	{
		foreach (var node in GetChildren())
		{
			if (node.GetType() != typeof(MsbtEntryPageHolder))
				continue;

			((MsbtEntryPageHolder)node).SetupButtonActiveness();
		}
	}
}
