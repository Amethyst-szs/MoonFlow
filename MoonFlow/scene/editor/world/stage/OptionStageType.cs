using Godot;
using System;
using System.Text.RegularExpressions;

using MoonFlow.Project.Database;
using System.Linq;

using CSExtensions;

namespace MoonFlow.Scene.EditorWorld;

public partial class OptionStageType : OptionButton
{
	private static readonly string[] Options =
		Enum.GetNames(typeof(StageInfo.CatEnum)).Select(s => s.SplitCamelCase()).ToArray();

	public override void _Ready()
	{
		foreach (var option in Options)
		{
			if (option != "Unknown")
				AddItem(option);
		}
	}
}
