using System;
using System.Threading.Tasks;
using Godot;

using MoonFlow.Scene;

namespace MoonFlow.Project;

public class ProjectState(string path, ConfigFile config, ProjectLoading loadScreen)
{
    // Initilzation properties
    public string Path { get; private set; } = path;
    private ConfigFile Config = config;
    private ProjectLoading LoadingScreen = loadScreen;

    // Status
    private bool IsInitComplete = false;

    // Project Components
    public ProjectLMSHolder HolderLMS = null;

    public void InitProject()
    {
        // Complete Initilization
        Config = null;
        LoadingScreen = null;
        IsInitComplete = true;
        LoadingScreen.LoadingComplete();
    }

    public bool IsReady() { return IsInitComplete; }
}