using System;
using System.IO;
using System.Linq;
using Godot;
using MoonFlow.Scene;
using MoonFlow.Scene.Home;
using MoonFlow.Scene.Main;

namespace MoonFlow.Project;

public static class ProjectManager
{
    private static ProjectState Project = null;
    private const string ProjectFileName = "project.moonflow";

    public static MainSceneRoot SceneRoot { get; set; } = null;

    // ====================================================== //
    // ============== Open Project by Directory ============= //
    // ====================================================== //

    public enum OpenProjectResult
    {
        OK,
        INVALID_PATH,
        NO_PROJECT_FILE,
        INVALID_PROJECT_FILE,
    }

    public static OpenProjectResult TryOpenProject(string path)
    {
        // If there isn't a reference to MoonFlow's MainSceneRoot node at the top of the godot scene, throw exception
        if (SceneRoot == null)
            throw new NullReferenceException("ProjectManager cannot open project without MainSceneRoot loaded!");

        // Ensure this path isn't a romfs directory in the RomfsAccessor
        if (RomfsAccessor.VersionDirectories.ContainsValue(path))
            return OpenProjectResult.INVALID_PATH;

        // If the selected directory contains a romfs folder, navigate into this folder
        path = path.TrimEnd('/');

        if (Directory.Exists(path + "/romfs"))
            path += "/romfs";

        // Navigate to project file
        var projectPath = path;

        // If this directory does not contain a LocalizedData/Common folder, return
        if (!Directory.Exists(projectPath + "/LocalizedData/Common/"))
            return OpenProjectResult.INVALID_PATH;
        else
            projectPath += "/LocalizedData/Common/";

        // Check if this path contains a project config file
        if (!File.Exists(projectPath + ProjectFileName))
            return OpenProjectResult.NO_PROJECT_FILE;

        // Read in config file
        var config = new ConfigFile();
        Error err = config.Load(projectPath + ProjectFileName);

        if (err != Error.Ok)
            return OpenProjectResult.INVALID_PROJECT_FILE;

        // Close the FrontDoor application if open and open the project loading screen
        var frontDoor = SceneRoot.GetApp<FrontDoor>();
        frontDoor?.AppClose(true);

        var loadScreen = SceneCreator<ProjectLoading>.Create();
        SceneRoot.NodeApps.AddChild(loadScreen);

        // Initilize project
        Project = new(path, config, loadScreen);
        return OpenProjectResult.OK;
    }
}