using System.Text.RegularExpressions;
using Godot;

using Nindot.LMS.Msbp;

namespace Godot.Extension;

public static partial class Extension
{
	public static Color ToGodotColor(this BlockColor.Entry entry)
	{
		return Color.Color8(entry.R, entry.G, entry.B, entry.A);
	}
	public static BlockColor.Entry ToMsbpColor(this Color c)
	{
		return new BlockColor.Entry((byte)c.R8, (byte)c.G8, (byte)c.B8, (byte)c.A8);
	}
}