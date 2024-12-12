using Godot;
using System;

using MoonFlow.Project.Database;
using MoonFlow.Scene.EditorMsbt;
using MoonFlow.Scene.Home;

namespace MoonFlow.Scene.EditorWorld;

public partial class VBoxScenarioInfo : InfoBoxBase
{
	[Export]
	private SpinBox SpinCount;
	[Export]
	private SpinBox SpinWorldPeace;
	[Export]
	private SpinBox SpinPostgame;
	[Export]
	private SpinBox SpinMoonRock;

    public override void OpenWorld(WorldInfo world)
	{
		base.OpenWorld(world);

		SpinCount.Value = world.ScenarioNum;
		SpinWorldPeace.Value = world.ClearMainScenario;
		SpinPostgame.Value = world.AfterEndingScenario;
		SpinMoonRock.Value = world.MoonRockScenario;
	}

	#region Signals

	private void OnSetScenarioCount(int id)
	{
		Info.ScenarioNum = id;
		EmitSignal(SignalName.ModifiedWorldInfo);
	}
	private void OnSetScenarioWorldPeace(int id)
	{
		Info.ClearMainScenario = id;
		EmitSignal(SignalName.ModifiedWorldInfo);
	}
	private void OnSetScenarioPostgame(int id)
	{
		Info.AfterEndingScenario = id;
		EmitSignal(SignalName.ModifiedWorldInfo);
	}
	private void OnSetScenarioMoonRock(int id)
	{
		Info.MoonRockScenario = id;
		EmitSignal(SignalName.ModifiedWorldInfo);
	}

	#endregion
}
