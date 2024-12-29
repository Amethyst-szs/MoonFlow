using System;
using Godot;

using MoonFlow.Project;

namespace MoonFlow.Scene.Home;

[Icon("res://asset/app/icon/home.png"), ScenePath("res://scene/home/home.tscn")]
public partial class HomeRoot : AppScene
{
    private static readonly GDScript DropdownButton = GD.Load<GDScript>("res://addons/ui_node_ext/dropdown_checkbox.gd");

    public override bool TryCloseFromTreeQuit(out SignalAwaiter awaiter)
    {
        awaiter = null;
        return true;
    }

    #region Node Utility

    public static void RecursiveFileSearch(Control root, string term)
	{
		if (root is Button button)
		{
			if (root.GetScript().As<Script>() != DropdownButton)
			{
				root.Visible = root.Name.ToString().Contains(term, StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				root.Visible = term == string.Empty;
				button.SetPressedNoSignal(false);

				var dropdownChild = root.Get("dropdown").As<Control>();
				if (dropdownChild != null)
					dropdownChild.Visible = !root.Visible;
			}
		}

		if (root is HSeparator)
			root.Visible = term == string.Empty;
        
        if (term != string.Empty && root is MarginContainer)
            root.Visible = IsAnyChildVisible<Button>(root);
		
		if (root.GetChildCount() == 0)
			return;
		
		foreach (var child in root.GetChildren())
		{
			if (child.GetType().IsSubclassOf(typeof(Control)))
				RecursiveFileSearch(child as Control, term);
		}
	}

    public static bool IsAnyChildVisible<T>(Control initialRoot, Control root = null)
    {
        root ??= initialRoot;

        if (root != initialRoot && root.Visible && root.GetType() == typeof(T))
            return true;

        foreach (var child in root.GetChildren())
            if (IsAnyChildVisible<T>(initialRoot, child as Control))
                return true;
        
        return false;
    }

    #endregion
}
