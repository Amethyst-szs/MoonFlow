using Godot;
using System;
using System.Text.RegularExpressions;

using MoonFlow.Project.Database;
using System.Linq;

namespace MoonFlow.Scene.EditorWorld;

public partial class OptionStageType : OptionButton
{
	private static readonly string[] Options =
		Enum.GetNames(typeof(StageInfo.CatEnum)).Select(SplitCamelCase).ToArray();

	public override void _Ready()
	{
		foreach (var option in Options)
		{
			if (option != "Unknown")
				AddItem(option);
		}
	}

	public static string SplitCamelCase(string str)
	{
		return Regex.Replace(
			Regex.Replace(
				str,
				@"(\P{Ll})(\P{Ll}\p{Ll})",
				"$1 $2"
			),
			@"(\p{Ll})(\P{Ll})",
			"$1 $2"
		);
	}
}
