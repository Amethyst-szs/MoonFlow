using Godot;

using MoonFlow.Project.Database;

namespace MoonFlow.Scene.EditorWorld;

public partial class OptionStageType : OptionButton
{
	public override void _Ready()
	{
		foreach (var option in StageInfo.CategoryNames)
		{
			if (option != "Unknown")
				AddItem(option);
		}
	}
}
