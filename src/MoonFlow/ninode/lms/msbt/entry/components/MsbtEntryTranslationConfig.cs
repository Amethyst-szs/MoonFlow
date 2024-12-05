using Godot;
using System;

using MoonFlow.Project;

[ScenePath("res://ninode/lms/msbt/entry/components/msbt_entry_translation_config.tscn")]
public partial class MsbtEntryTranslationConfig : PanelContainer
{
	[Signal]
	public delegate void SyncToggledEventHandler(bool isDisableSync);

	public void SetupNode(ProjectLanguageMetaHolder.Meta meta)
	{
		SetButtonState(meta.IsDisableSync);
	}

	private void OnTranslationSyncToggled(bool isDisableSync)
	{
		SetButtonState(isDisableSync);
		EmitSignal(SignalName.SyncToggled, isDisableSync);
	}

	private void SetButtonState(bool state)
	{
		var button = GetNode<CheckBox>("%Check_Sync");
		button.ButtonPressed = state;
	}
}