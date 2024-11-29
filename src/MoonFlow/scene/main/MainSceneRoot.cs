using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using MoonFlow.Project;

namespace MoonFlow.Scene.Main;

public partial class MainSceneRoot : VBoxContainer
{
    public HBoxContainer NodeHeader = null;
    public Control NodeApps = null;
    public Taskbar NodeTaskbar = null;

    public override void _Ready()
    {
        NodeHeader = GetNode<HBoxContainer>("%Header");
        NodeApps = GetNode<Control>("%Apps");
        NodeTaskbar = GetNode<Taskbar>("%Taskbar");

        // Add self-reference to ProjectManager
        ProjectManager.SceneRoot = this;
        TreeExiting += OnTreeExiting;

        // Connect to updates in the NodeApps control
        NodeApps.ChildEnteredTree += OnAppTreeChanged;
        NodeApps.ChildExitingTree += OnAppTreeChanged;

        // Create starting scene
        var frontDoor = SceneCreator<FrontDoor>.Create();
        NodeApps.AddChild(frontDoor);
    }

    private static void OnTreeExiting()
    {
        // Remove node reference in project manager
        ProjectManager.SceneRoot = null;
    }

    public IEnumerable<AppScene> GetApps()
    {
        return NodeApps.GetChildren().Cast<AppScene>();
    }

    // Returns first open app of type T
    public AppScene GetApp<T>()
    {
        foreach (var app in GetApps())
        {
            if (app.GetType() == typeof(T))
                return app;
        }

        return null;
    }

    public AppScene GetActiveApp()
    {
        foreach (var app in GetApps())
        {
            if (app.Visible)
                return app;
        }

        return null;
    }

    private void OnAppTreeChanged(Node _)
    {
        // Check if the front door is open to determine header visiblity
        foreach (var app in NodeApps.GetChildren())
        {
            if (app.GetType() != typeof(FrontDoor))
                continue;

            NodeHeader.Hide();
            return;
        }

        NodeHeader.Show();
    }
}
