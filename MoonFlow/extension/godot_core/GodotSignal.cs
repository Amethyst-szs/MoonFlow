using System.Text.RegularExpressions;
using Godot;

using Nindot.LMS.Msbp;

namespace MoonFlow.Ext;

public static partial class Extension
{
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