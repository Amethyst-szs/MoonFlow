using Godot;
using System;
using System.Text.RegularExpressions;

using MoonFlow.Project.Database;
using System.Linq;

using MoonFlow.Ext;

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
