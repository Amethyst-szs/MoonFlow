using Godot;
using MoonFlow.Project;
using MoonFlow.Scene.Main;
using MoonFlow.Scene.Settings;

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
		
		msbt.Editor.ContainerMassAddLabels.Show();
	}
}
