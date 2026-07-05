using System;
using System.Threading.Tasks;
using Godot;

namespace MoonFlow.Scene.Home;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

[SceneUid("uid://c5b01vfsh7pwp"), Icon("res://asset/app/icon/home.png")]
public partial class HomeRoot : AppScene
{
	public override async Task<bool> TryCloseFromTreeQuit()
	{
		return true;
	}

	#region Node Utility

	public static async void SetVisibleIfAnyChildVisible<T>(Control root)
	{
		await Extension.WaitProcessFrame(root);
		root.Visible = IsAnyChildVisible<T>(root);
	}

	public static bool IsAnyChildVisible<T>(Control root)
	{
		foreach (var child in root.GetChildren())
			if (IsAnyChildVisibleRecursive<T>(child as Control))
				return true;

		return false;
	}
	private static bool IsAnyChildVisibleRecursive<T>(Control root)
	{
		if (root == null)
			return false;

		if (root.Visible && root.GetType() == typeof(T))
			return true;

		foreach (var child in root.GetChildren())
			if (IsAnyChildVisibleRecursive<T>(child as Control))
				return true;

		return false;
	}

	#endregion
}

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously