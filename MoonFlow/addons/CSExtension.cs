using System.Text.RegularExpressions;
using Godot;

namespace CSExtensions
{
	public static partial class Extension
	{
		public static string SplitCamelCase(this string str)
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

		public static void DeselectAllButtons(this Node node)
		{
			if (node is not Control)
				return;

			if (node.GetType() == typeof(Button))
				(node as Button).SetPressedNoSignal(false);

			if (node.GetChildCount() == 0)
				return;

			foreach (var child in node.GetChildren())
				DeselectAllButtons(child);
		}
	}
}