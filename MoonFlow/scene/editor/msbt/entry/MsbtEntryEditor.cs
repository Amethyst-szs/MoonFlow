using Godot;
using System;

using Nindot.LMS.Msbt;

using MoonFlow.Project;

namespace MoonFlow.Scene.EditorMsbt;

public partial class MsbtEntryEditor(MsbtEditor parent, MsbtEntry entry, ProjectLanguageFileEntryMeta meta) : VBoxContainer
{
	private readonly MsbtEditor Parent = parent;
	public readonly MsbtEntry Entry = entry;
	public MsbtEntry EntrySourceLanguage { get; private set; }
	public readonly ProjectLanguageFileEntryMeta Metadata = meta;

	public bool IsTranslationMode { get; private set; } = false;

	[Signal]
	public delegate void EntryModifiedEventHandler(MsbtEntryEditor entry);

	public override void _Ready()
	{
		this.QueueFreeAllChildren();

		// If the current language doesn't match the default language, create translation config header
		if (!Parent.IsDefaultLanguage())
		{
			var langConfig = SceneCreator<MsbtEntryTranslationConfig>.Create();
			langConfig.SetupNode(Metadata);

			langConfig.Connect(MsbtEntryTranslationConfig.SignalName.SyncToggled,
				Callable.From(new Action<bool>(OnSyncToggled)));

			AddChild(langConfig);
		}

		// Get access to the desired entry in the default language
		EntrySourceLanguage = Parent.FileList[Parent.DefaultLanguage].GetEntry(Entry.Name);

		// Setup pages and page separators
		BuildSeparator(-1);
		for (int i = 0; i < Entry.Pages.Count; i++)
		{
			// Access page from current language and default language
			MsbtPage page = Entry.Pages[i];
			MsbtPage pageSourcePreview = page;

			if (EntrySourceLanguage.Pages.Count > i)
				pageSourcePreview = EntrySourceLanguage.Pages[i];
			else
				pageSourcePreview = null;

			// Setup page holder
			var holder = SceneCreator<MsbtEntryPageHolder>.Create();

			holder.Connect(MsbtEntryPageHolder.SignalName.PageDelete,
				Callable.From(new Action<MsbtPageEditor>(OnDeletePage)));

			holder.Connect(MsbtEntryPageHolder.SignalName.PageOrganize,
				Callable.From(new Action<MsbtPageEditor, int>(OnOrganizePage)));

			holder.Connect(MsbtEntryPageHolder.SignalName.PageModified,
				Callable.From(new Action<MsbtPageEditor>(OnModifiedPage)));

			holder.Connect(MsbtEntryPageHolder.SignalName.DebugHashCopy, Callable.From(OnDebugHashCopy));

			holder.Init(Parent.Project, page, pageSourcePreview);
			AddChild(holder);
			BuildSeparator(i);
		}

		// Complete setup
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

	private void OnDebugHashCopy()
	{
		var hash = ProjectLanguageMetaHolder.CalcHash(Parent.File.Sarc.Name, Parent.File.Name, Entry.Name);
		DisplayServer.ClipboardSet(hash);

		GD.Print(hash + " added to system clipboard!");
	}

	private void OnSyncToggled(bool isDisableSync)
	{
		// Set flags
		Metadata.IsDisableSync = isDisableSync;
		SetModified();

		// If enabling source syncing, reset the entry
		if (!isDisableSync)
		{
			Metadata.IsMod = false;

			Entry.Pages.Clear();
			EntrySourceLanguage.Pages.ForEach(p => Entry.Pages.Add(p.Clone()));

			_Ready();
		}

		// Update button states
		UpdatePageOrderingButtons();
	}

	// ====================================================== //
	// ====================== Utilities ===================== //
	// ====================================================== //

	public void SetModified()
	{
		Entry.SetModifiedFlag();
		Metadata.IsMod = true;

		EmitSignal(SignalName.EntryModified, this);
	}
	public void SetTranslationMode() { IsTranslationMode = true; }

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
		foreach (var child in GetChildren())
		{
			switch (child)
			{
				case MsbtEntryPageHolder:
					((MsbtEntryPageHolder)child).UpdateButtonActiveness(Metadata.IsDisableSync, Parent.IsDefaultLanguage());
					continue;
				case MsbtEntryPageSeparator:
					((MsbtEntryPageSeparator)child).UpdateAddButtonState(Metadata.IsDisableSync, Parent.IsDefaultLanguage());
					continue;
			}
		}
	}
}
