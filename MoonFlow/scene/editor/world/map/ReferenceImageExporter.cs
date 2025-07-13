using Godot;
using MoonFlow.Project;
using MoonFlow.Project.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoonFlow.Scene.EditorWorld;

[SceneUid("uid://bketxyjx13sfi")]
public partial class ReferenceImageExporter : SubViewport
{
    private TabMap Child = null;

    [Export]
    private Texture2D ExampleOutputImage;

    public static async Task<Dictionary<string, Image>> CreateImage(WorldInfo world, int scenario)
    {
        // Create reference image exporter
        if (Engine.GetMainLoop() is not SceneTree tree)
            throw new Exception("Invalid type of main loop");

        var self = SceneCreator<ReferenceImageExporter>.Create();
        tree.Root.AddChild(self);

        // Ensure node is ready
        if (!self.IsNodeReady())
            throw new Exception("Image renderer is not ready!");

        await self.Child.InitMap(world, scenario);
        self.Child.DisableMapControls();

        // Store list of output images
        var output = new Dictionary<string, Image>();

        // Standard image with the background texture and all of the collectable icons
        await self.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        output.Add(ResolveFileName(world, scenario, "Full"), self.GetTexture().GetImage());

        // Image of only icons with the background rect completely hidden
        self.Child.SelfModulate = Colors.Transparent;

        await self.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        output.Add(ResolveFileName(world, scenario, "IconOnly"), self.GetTexture().GetImage());

        // Add example pregenerated image to end of table
        output.Add("RefEmptyExample.png", self.ExampleOutputImage.GetImage());

        // Destroy and return list of rendered textures
        self.QueueFree();
        return output;
    }
    public static async Task CreateAndSaveImage(WorldInfo world, int scenario)
    {
        var images = await CreateImage(world, scenario);

        DisplayServer.FileDialogShow(
			"Select output destination for reference images",
			ProjectManager.GetPath(),
			null,
			false,
			DisplayServer.FileDialogMode.OpenDir,
			[],
			Callable.From(new Action<bool, string[], int>((a, b, c) => SaveImages(images, a, b, c)))
		);
    }
    private static void SaveImages(Dictionary<string, Image> images, bool status, string[] paths, int filter)
    {
        if (!status || paths.Length != 1)
            return;

        string path = paths[0].Replace('\\', '/');
        if (!path.EndsWith('/'))
            path += '/';

        foreach (var image in images)
            image.Value.SavePng(path + image.Key);
    }

    private static string ResolveFileName(WorldInfo world, int scenario, string type)
    {
        return string.Format("Ref_{0}_{1}_{2}.png", world.Name, scenario, type);
    }

    public override void _Ready()
    {
        Child = GetChild<TabMap>(0);
    }
}
