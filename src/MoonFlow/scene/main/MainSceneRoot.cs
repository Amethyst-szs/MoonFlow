using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonFlow.Scene.Main;

public partial class MainSceneRoot : VBoxContainer
{
    public Taskbar NodeTaskbar = null;
    public Control NodeApps = null;

    public override void _Ready()
    {
        NodeTaskbar = GetNode<Taskbar>("%Taskbar");
        NodeApps = GetNode<Control>("%Apps");
    }

    public IEnumerable<AppScene> GetApps()
    {
        return NodeApps.GetChildren().Cast<AppScene>();
    }

    public AppScene GetActiveApp()
    {
        foreach (var app in NodeApps.GetChildren().Cast<AppScene>())
        {
            if (app.Visible)
                return app;
        }

        return null;
    }
}
