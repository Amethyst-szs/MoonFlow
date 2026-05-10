using Godot;
using MoonFlow.Project;
using System;

namespace MoonFlow.Scene.EditorMsbt;

[SceneUid("uid://bbovlwtl0g0pg")]
public partial class EntryLabelButton : Button
{
	public string EntryLabel { get; private set; }

	[Export, ExportGroup("Internal References")]
	private RichTextLabel LabelRich;
	[Export]
	private LineEdit LabelEditLine;

	[Signal]
    public delegate void EntryRenameRequestEventHandler(EntryLabelButton self, string newLabel);

	private ProjectLanguageMetaBucketEntry EntryMeta;
	private ProjectLanguageMetaBucketEntry EntryMetaSourceLang;

	public void SetupMetadata(ProjectLanguageMetaBucketEntry meta, ProjectLanguageMetaBucketEntry sourceLangMeta)
	{
		EntryMeta = meta;
		EntryMetaSourceLang = sourceLangMeta;
	}

	public void SetupButton(EntryListBase parent, string key, string label, bool isStageMessage)
	{
		LabelRich.Show();
		LabelEditLine.Hide();

		EntryLabel = key;
		Name = key;

		UpdateIcon();

		Connect(SignalName.Pressed, Callable.From(() => parent.OnEntrySelected(EntryLabel)));
		Connect(SignalName.ButtonDown, Callable.From(() => parent.OnEntrySelected(EntryLabel)));
		Connect(SignalName.MouseEntered, Callable.From(() => parent.OnEntryHovered(EntryLabel)));
		Connect(SignalName.EntryRenameRequest, Callable.From(new Action<EntryLabelButton, string>(parent.OnEntryRenameRequest)));
		
		// Temporary implementation before larger refactor
		LabelRich.Text = label;
		return;

		// Attempt to break into object and parameter parts
		// var components = label.Split('_', StringSplitOptions.RemoveEmptyEntries);
		// if (components.Length != 2)
		// {
		// 	LabelRich.Text = label;
		// 	return;
		// }

		// var parameter = components[0];
		// var obj = components[1];

		// LabelRich.AddText(obj);

		// LabelRich.PushFontSize(14);
		// LabelRich.AddText(string.Format(" - {0}", parameter));
		// LabelRich.PopAll();
	}

	#region Rename

	public void BeginEntryLabelRename()
	{
		LabelRich.Hide();
		LabelEditLine.Show();
		LabelEditLine.Text = EntryLabel;
		LabelEditLine.GrabFocus();
	}
	public void EndEntryLabelRename()
	{
		LabelRich.Show();
		LabelEditLine.Hide();

		if (LabelEditLine.Text != EntryLabel)
			EmitSignalEntryRenameRequest(this, LabelEditLine.Text);
	}
	public void EndEntryLabelRenameCancel()
	{
		LabelRich.Show();
		LabelEditLine.Hide();
	}
	public void OnRenameRequestCompleted(string newName)
	{
		EntryLabel = newName;
		Name = newName;
		LabelRich.Text = newName;
	}

	#endregion

	#region Status Icons

	[Export, ExportGroup("Status")]
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
