using System;
using Godot;

using Nindot.LMS.Msbt.TagLib;

using MoonFlow.Project;

namespace MoonFlow.LMS.Msbt;

public static class TagEditIconCommon
{
    public static TagSubmenuBase InitSubmenu<T>(MsbtTagElement tag)
    {
        if (!typeof(T).IsSubclassOf(typeof(TagSubmenuBase)))
			throw new Exception(typeof(T).Name + " is invalid type!");

		var menu = SceneCreator<T>.Create();
		var scene = ProjectManager.SceneRoot;

		var menuBase = menu as TagSubmenuBase;
		scene.AddChild(menuBase);

		menuBase.InitSubmenu();
        return menuBase;
    }
}