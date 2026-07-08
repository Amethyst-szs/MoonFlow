using Godot;
using System;

using MoonFlow.Async;
using MoonFlow.Project;
using MoonFlow.Project.Database;
using MoonFlow.Scene.EditorMsbt;

namespace MoonFlow.Scene.Home;

public partial class TabProject : VBoxContainer
{
    private void OnGenerateDatabaseCheckpointFlag()
	{
		AsyncRunner.Run(GenerateDatabaseCheckpointFlag, AsyncDisplay.Type.GenerateCheckpointDb, "dbgen_checkpoint_flag");
	}
	private void GenerateDatabaseCheckpointFlag(AsyncDisplay display)
	{
		var db = ProjectManager.GetDB();
		CheckpointFlagDbGenerator.Generate(db, display.UpdateProgress);
	}

    private static void OnOpenMsbpColorEditor() { AppSceneServer.CreateApp<MsbpColorEditor>(); }
}
