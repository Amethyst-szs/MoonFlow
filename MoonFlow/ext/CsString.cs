using System.Text.RegularExpressions;
using Godot;

using Nindot.LMS.Msbp;

namespace MoonFlow.Ext;

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
}