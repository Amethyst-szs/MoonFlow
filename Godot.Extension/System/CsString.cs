using System.Text.RegularExpressions;

namespace Godot.Extension;

public static partial class Extension
{
	[GeneratedRegex(@"[.:@/:%]")]
	private static partial Regex ToNodeNameRegex();

	// Replaces .:@/"% in string with underscores to support formatting of Godot.Node.Name
	public static string ToNodeName(this string str)
	{
		return ToNodeNameRegex().Replace(str, "_");
	}

	[GeneratedRegex(@"(\p{Ll})(\P{Ll})")]
	private static partial Regex CamelReg1();
	[GeneratedRegex(@"(\P{Ll})(\P{Ll}\p{Ll})")]
	private static partial Regex CamelReg2();
	public static string SplitCamelCase(this string str)
	{
		return CamelReg1().Replace(CamelReg2().Replace(str, "$1 $2"), "$1 $2");
	}

	public static string RemoveFileExtension(this string str)
	{
		int idx = str.LastIndexOf('.');
		if (idx != -1) return str[..idx];
		return str;
	}
}