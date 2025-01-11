using System.Text.RegularExpressions;
using Godot;

using Nindot.LMS.Msbp;

namespace MoonFlow.Ext;

public static partial class Extension
{
	public static void DeselectAllButtons(this Node node, bool isEmitSignal = false)
	{
		if (node is not Control)
			return;

		if (node.GetType() == typeof(Button))
		{
			if (isEmitSignal)
				(node as Button).ButtonPressed = false;
			else
				(node as Button).SetPressedNoSignal(false);
		}

		if (node.GetChildCount() == 0)
			return;

		foreach (var child in node.GetChildren())
			DeselectAllButtons(child);
	}
}