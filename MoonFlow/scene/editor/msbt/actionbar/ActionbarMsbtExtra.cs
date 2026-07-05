using System;
using Godot;

using MoonFlow.Scene.Main;

namespace MoonFlow.Scene.EditorMsbt;

public partial class ActionbarMsbtExtra : ActionbarItemBase
{
	private enum MenuIds : int
	{
		MASS_ADD_LABELS = 0,
	}

	public override void _Ready()
	{
		base._Ready();

		AssignFunction((int)MenuIds.MASS_ADD_LABELS, OnPressMassSearchLabels);
	}

	private void OnPressMassSearchLabels()
	{
		AppScene app = AppSceneServer.GetActiveApp();
		if (app is not MsbtAppHolder msbt)
			return;

		var popup = SceneCreator<WindowMassAddLabels>.Create();
		popup.Connect(WindowMassAddLabels.SignalName.TextSubmitted,
			Callable.From(new Action<string>(msbt.Editor.OnMassImportLabelsToolSubmit))
		);

		msbt.Editor.AddChild(popup);
		popup.PopupCentered();
	}
}
