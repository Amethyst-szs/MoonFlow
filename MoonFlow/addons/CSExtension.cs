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

		public static void TryUpdateSignal(this Node node, string signal, bool isConnect, Callable callable)
		{
			bool isCon = node.IsConnected(signal, callable);

			if (!isCon && isConnect)
				node.Connect(signal, callable);
			else if (isCon && !isConnect)
				node.Disconnect(signal, callable);
		}

		public static void TryConnect(this Node node, string signal, Callable callable)
		{
			TryUpdateSignal(node, signal, true, callable);
		}
		public static void TryDisconnect(this Node node, string signal, Callable callable)
		{
			TryUpdateSignal(node, signal, false, callable);
		}
	}
}