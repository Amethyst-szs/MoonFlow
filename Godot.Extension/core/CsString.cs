using System.Text.RegularExpressions;

namespace Godot.Extension;

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

	public static string RemoveFileExtension(this string str)
	{
		int idx = str.LastIndexOf('.');
		if (idx != -1) return str[..idx];
		return str;
	}
}