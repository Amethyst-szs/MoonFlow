using Godot;
using System;

using Nindot.LMS.Msbt;

using MoonFlow.Project;

namespace MoonFlow.Scene.EditorMsbt;

public partial class MsbtEntryEditor(MsbtEditor parent, MsbtEntry entry, ProjectLanguageFileEntryMeta meta) : VBoxContainer
{
	private readonly MsbtEditor Parent = parent;
	public readonly MsbtEntry Entry = entry;
	public readonly ProjectLanguageFileEntryMeta Metadata = meta;

	[Signal]
	public delegate void EntryModifiedEventHandler(MsbtEntryEditor entry);

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

			holder.Connect(MsbtEntryPageHolder.SignalName.PageModified,
				Callable.From(new Action<MsbtPageEditor>(OnModifiedPage)));

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
		SetModified();

		Entry.Pages.Remove(page.Page);

		foreach (var child in GetChildren())
			child.QueueFree();

		GetTree().CreateTimer(0).Timeout += _Ready;
	}

	private void OnOrganizePage(MsbtPageEditor page, int offset)
	{
		SetModified();

		int index = Entry.Pages.IndexOf(page.Page);

		Entry.Pages.Remove(page.Page);
		Entry.Pages.Insert(index + offset, page.Page);

		foreach (var child in GetChildren())
			child.QueueFree();

		GetTree().CreateTimer(0).Timeout += _Ready;
	}

	private void OnAddPage(int index)
	{
		SetModified();

		Entry.Pages.Insert(index + 1, []);

		foreach (var child in GetChildren())
			child.QueueFree();

		GetTree().CreateTimer(0).Timeout += _Ready;
	}

	private void OnModifiedPage(MsbtPageEditor page) { SetModified(); }

	private void OnSyncToggled(bool isDisableSync)
	{
		SetModified();

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

	public void SetModified()
	{
		Entry.SetModifiedFlag();
		Metadata.IsMod = true;
		
		EmitSignal(SignalName.EntryModified, this);
	}
}
