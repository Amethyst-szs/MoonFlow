using System;
using System.IO;
using FluentFTP.Helpers;
using Godot;
using MoonFlow.Async;
using MoonFlow.Project;
using MoonFlow.Project.Database;
using MoonFlow.Scene.Settings;

namespace MoonFlow.Scene.Main;

public partial class ActionbarDatabases : ActionbarItemBase
{
	private enum MenuIds : int
	{
		DATABASES_CHECKPOINT = 0,
	}

	public override void _Ready()
	{
		base._Ready();

		AssignFunction((int)MenuIds.DATABASES_CHECKPOINT, OnGenerateDatabaseCheckpointFlag);
	}

	private void OnGenerateDatabaseCheckpointFlag()
	{
		AsyncRunner.Run(GenerateDatabaseCheckpointFlag, AsyncDisplay.Type.GenerateCheckpointDb);
	}
	private void GenerateDatabaseCheckpointFlag(AsyncDisplay display)
	{
		var db = ProjectManager.GetDB();
		CheckpointFlagDbGenerator.Generate(db, display.UpdateProgress);
	}
}
