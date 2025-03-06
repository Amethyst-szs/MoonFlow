using Godot;

using MoonFlow.Project;

[SceneUid("uid://brr5d6o7cfwd")]
public partial class MsbtEntryTranslationConfig : PanelContainer
{
	[Signal]
	public delegate void SyncToggledEventHandler(bool isDisableSync);

	public void SetupNode(ProjectLanguageMetaBucketEntry meta)
	{
		SetButtonState(meta.OffSync);
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
