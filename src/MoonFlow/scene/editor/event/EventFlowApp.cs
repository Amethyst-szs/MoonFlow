using Godot;
using System;
using System.Linq;

using Nindot;
using Nindot.Al.EventFlow;
using Nindot.Al.EventFlow.Smo;

using MoonFlow.Async;
using MoonFlow.Scene.Main;
using System.Threading.Tasks;

namespace MoonFlow.Scene.EditorEvent;

[ScenePath("res://scene/editor/event/event_flow_app.tscn")]
public partial class EventFlowApp : AppScene
{
    #region Properties

    // ~~~~~~~~~~~ Event Flow Graph ~~~~~~~~~~ //

    public SarcEventFlowGraph Graph { get; private set; } = null;
    public GraphMetaHolder MetadataHolder { get; private set; } = null;
    public GraphMetadata Metadata { get { return MetadataHolder.Data; } }

    // ~~~~~~~~~ Internal References ~~~~~~~~~ //

    [Export, ExportGroup("Internal References")]
    private GraphCanvas GraphCanvas = null;
    [Export]
    private CanvasLayer BackgroundCanvas = null;
    [Export]
    private Node2D GraphNodeHolder = null;

    #endregion

    #region Initilization

    protected override void AppInit()
    {
        // Connect to signal events
        VisibilityChanged += OnVisiblityChanged;

        Scene.NodeHeader.Connect(Header.SignalName.ButtonSave, Callable.From(SaveFile));

        // DEBUG SHIT
        OpenFile(SarcFile.FromFilePath("C:/Users/evils/AppData/Roaming/Godot/app_userdata/MoonFlow/debug/romfs/EventData/Common.szs"), "SimpleMessage.byml");
    }

    private async void InitEditor()
    {
        // Destroy current contents of editor, if any exist
        foreach (var child in GraphNodeHolder.GetChildren())
        {
            GraphNodeHolder.RemoveChild(child);
            child.QueueFree();
        }

        await InitNodeList();
        InitEntryPointNodes();
    }

    private async Task InitNodeList()
    {
        // Initilize nodes from the Graph data container
        foreach (var node in Graph.Nodes.Values)
        {
            if (node.Id == int.MinValue)
                throw new EventFlowException("Node initilized without an Id!");

            // Create node
            var nodeEdit = SceneCreator<EventFlowNodeCommon>.Create();
            GraphNodeHolder.AddChild(nodeEdit);

            // Setup metadata access (Node position, comments, and other additional info)
            Metadata.Nodes.TryGetValue(node.Id, out NodeMetadata data);
            nodeEdit.InitContentMetadata(Metadata, data);

            // Init main content from event flow graph byml
            nodeEdit.InitContent(node, Graph);
        }

        await ToSignal(Engine.GetMainLoop(), "process_frame");

        // Setup node port connections
        foreach (var n in GraphNodeHolder.GetChildren())
        {
            var nodeEdit = n as EventFlowNodeCommon;

            var idList = nodeEdit.Content.GetNextIds();
            if (idList.Length == 0)
                continue;

            var list = idList.Select(s =>
            {
                if (s == int.MinValue) return null;
                return GraphNodeHolder.GetChild(s) as EventFlowNodeCommon;
            });

            nodeEdit.SetupConnections(list.ToList());
        }

        await ToSignal(Engine.GetMainLoop(), "process_frame");
    }

    private void InitEntryPointNodes()
    {
        foreach (var entry in Graph.EntryPoints)
        {
            var target = GraphNodeHolder.GetNode<EventFlowNodeCommon>(entry.Value.Id.ToString());
            if (!IsInstanceValid(target))
            {
                GD.PushError("Failed to initilize " + entry.Key + " entry point");
                return;
            }

            // Create node
            var entryEdit = SceneCreator<EventFlowEntryPoint>.Create();
            GraphNodeHolder.AddChild(entryEdit);

            // Init main content from event flow graph byml
            entryEdit.InitContent(entry.Key, Graph, target);

            // Setup metadata access (Node position, comments, and other additional info)
            Metadata.EntryPoints.TryGetValue(entry.Key, out NodeMetadata data);
            entryEdit.InitContentMetadata(Metadata, data);
        }
    }

    #endregion

    #region Read & Write

    public void OpenFile(SarcFile sarc, string name)
    {
        Graph = sarc.GetFileEventFlow(name, new ProjectSmoEventFlowFactory());
        MetadataHolder = GraphMetaHolder.Create(Graph);
        InitEditor();
    }

    public async void SaveFile()
    {
        GD.Print("\n - Saving ", Graph.Name);
        var run = AsyncRunner.Run(TaskRunWriteFile, AsyncDisplay.Type.SaveEventFlowGraph);

        await run.Task;
        await ToSignal(Engine.GetMainLoop(), "process_frame");

        if (run.Task.Exception == null)
            GD.Print("Saved ", Graph.Name);
        else
            GD.Print("Saving failed for ", Graph.Name);
    }

    public void TaskRunWriteFile(AsyncDisplay display)
    {
        // Write graph event data
        display.UpdateProgress(0, 2);
        Graph.WriteArchive();

        // Write metadata holder
        display.UpdateProgress(1, 2);
        MetadataHolder.WriteFile();

        // Reset flag
        display.UpdateProgress(2, 2);
        IsModified = false;
    }

    #endregion

    #region Signals

    private void OnVisiblityChanged()
    {
        GraphCanvas.Visible = Visible;
        BackgroundCanvas.Visible = Visible;
    }

    private void OnFileModified() { IsModified = true; }

    #endregion
}
