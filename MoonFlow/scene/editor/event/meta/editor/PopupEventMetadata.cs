using Godot;
using System;

namespace MoonFlow.Scene.EditorEvent;

[ScenePath("res://scene/editor/event/meta/editor/popup_event_metadata.tscn")]
public partial class PopupEventMetadata : Popup
{
	public void SetupPopup(EventFlowNodeCommon target)
	{
		PopupCentered();
	}
}
