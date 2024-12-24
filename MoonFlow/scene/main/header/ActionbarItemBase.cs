using Godot;
using MoonFlow.Project;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoonFlow.Scene.Main;

public abstract partial class ActionbarItemBase : PopupMenu
{
    private Dictionary<int, Action> FuncLookup = [];
    protected Header Header = null;

    public override async void _Ready()
    {
        IdPressed += OnIdPressed;

        var scene = await GetScene();
        
        Header = scene.NodeHeader;
        Header.Connect(Header.SignalName.AppFocused, Callable.From(AppFocusChanged));
    }

    protected virtual void AppFocusChanged()
    {
        bool isValid = ProjectManager.IsProjectExist();

		for (var i = 0; i < ItemCount; i++)
			SetItemDisabled(i, !isValid);
    }

    private void OnIdPressed(long id)
    {
        if (!FuncLookup.TryGetValue((int)id, out Action value))
        {
            GD.PushWarning("No function found for menu item ", id);
            return;
        }

        value.Invoke();
    }

    protected void AssignFunction(int id, Action func, string shortcut = null)
    {
        if (shortcut != null)
            AssignShortcut(id, shortcut);
        
        FuncLookup[id] = func;
    }

    private void AssignShortcut(int id, string actionName)
	{
		var idx = GetItemIndex((int)id);

		var action = new InputEventAction { Action = actionName };

		var shortcut = new Shortcut();
		shortcut.Events.Add(action);
		SetItemShortcut(idx, shortcut);
	}

    protected async Task<MainSceneRoot> GetScene()
    {
        var sceneBase = GetTree().CurrentScene;
        if (sceneBase.GetType() != typeof(MainSceneRoot))
            throw new Exception("Invalid scene type!");
        
        var scene = (MainSceneRoot)sceneBase;
        if (!scene.IsNodeReady())
            await ToSignal(scene, SignalName.Ready);
        
        return scene;
    }
}
