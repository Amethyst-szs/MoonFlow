using Godot;
using MoonFlow.Project;
using System;

namespace MoonFlow.Scene.EditorMsbt;

[ScenePath("res://scene/editor/msbt/list/label/entry_label_button.tscn")]
public partial class EntryLabelButton : Button
{
	public string EntryLabel { get; private set; }

	private ProjectLanguageMetaBucketEntry EntryMeta;
	private ProjectLanguageMetaBucketEntry EntryMetaSourceLang;

	public void SetupButton(EntryListBase parent, string key, string label, ProjectLanguageMetaBucketEntry meta, ProjectLanguageMetaBucketEntry sourceLangMeta)
	{
		EntryMeta = meta;
		EntryMetaSourceLang = sourceLangMeta;

		EntryLabel = key;
		Name = key;
		Text = label;

		UpdateIcon();

		Connect(SignalName.Pressed, Callable.From(() => parent.OnEntrySelected(label)));
		Connect(SignalName.ButtonDown, Callable.From(() => parent.OnEntrySelected(label)));
		Connect(SignalName.MouseEntered, Callable.From(() => parent.OnEntryHovered(label)));
	}

	#region Status Icons

	[Export]
	private EntryLabelButtonState IconDefault;
	[Export]
	private EntryLabelButtonState IconUnsaved;
	[Export]
	private EntryLabelButtonState IconModified;

	[Export]
	private EntryLabelButtonState IconUnmodifiedInSourceLang;
	[Export]
	private EntryLabelButtonState IconUntranslated;

	private bool IsUnsavedChanges = false;

	public void SetUnsavedState(bool state)
	{
		IsUnsavedChanges = state;
		UpdateIcon();
	}

	public void UpdateIcon()
	{
		if (IsUnsavedChanges)
		{
			IconUnsaved.SetButtonToState(this);
			return;
		}

		if (EntryMeta == null || EntryMetaSourceLang == null)
			throw new NullReferenceException("Missing metadata access!");

		if (EntryMeta.Mod)
		{
			IconModified.SetButtonToState(this);
			return;
		}

		if (EntryMeta != EntryMetaSourceLang && EntryMetaSourceLang.Mod)
		{
			IconUntranslated.SetButtonToState(this);
			return;
		}

		if (EntryMeta != EntryMetaSourceLang && !EntryMetaSourceLang.Mod)
		{
			IconUnmodifiedInSourceLang.SetButtonToState(this);
			return;
		}

		IconDefault.SetButtonToState(this);
	}

	#endregion
}
