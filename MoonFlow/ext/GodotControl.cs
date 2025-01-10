using System;
using Godot;

namespace MoonFlow.Ext;

public static partial class Extension
{
    public static bool IsAnyChildFocused(this Control self)
    {
        foreach (var child in self.GetChildren())
        {
            if (child is not Control control)
                continue;
            
            if (control.HasFocus())
                return true;
            
            if (IsAnyChildFocused(control))
                return true;
        }

        return false;
    }
}