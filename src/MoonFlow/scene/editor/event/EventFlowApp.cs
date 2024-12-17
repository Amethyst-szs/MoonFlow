using Godot;
using System;

using Nindot;
using Nindot.Al.EventFlow;
using Nindot.Al.EventFlow.Smo;
using System.Linq;

namespace MoonFlow.Scene.EditorEvent;

[ScenePath("res://scene/editor/event/event_flow_app.tscn")]
public partial class EventFlowApp : AppScene
{
    #region Properties

    // ~~~~~~~~~~~ Event Flow Graph ~~~~~~~~~~ //

    public SarcEventFlowGraph Graph { get; private set; } = null;

    // ~~~~~~~~~ Internal References ~~~~~~~~~ //

    [Export, ExportGroup("Internal References")]
    private GraphCanvas GraphCanvas = null;
    [Export]
    private CanvasLayer BackgroundCanvas = null;
    [Export]
    private Node2D GraphNodeHolder = null;

    #endregion

    #region Initilization

    public override void _Ready()
    {
        VisibilityChanged += OnVisiblityChanged;

        // DEBUG SHIT
        OpenFile(SarcFile.FromFilePath("D:/NCA-NSP-XCI_TO_LayeredFS_v1.6/1.6/Super-Mario-Oddyesy/Odyssey100/romfs/EventData/Common.szs"), "SimpleMessage.byml");
    }

    private void InitEditor()
    {
        // Destroy current contents of editor, if any exist
        foreach (var child in GraphNodeHolder.GetChildren())
        {
            GraphNodeHolder.RemoveChild(child);
            child.QueueFree();
        }

        // Initilize every node in the EventFlowGraph
        foreach (var node in Graph.Nodes.Values)
        {
            if (node.Id == int.MinValue)
                throw new EventFlowException("Node initilized without an Id!");

            var edit = SceneCreator<EventFlowNode>.Create();
            GraphNodeHolder.AddChild(edit);
            edit.InitContent(node);

            edit.Position = new Vector2(node.Id * 100, node.Id * 30);
            edit.RawPosition = new Vector2(node.Id * 100, node.Id * 30);
        }

        // Setup node port connections
        foreach (var n in GraphNodeHolder.GetChildren())
        {
            var nodeEdit = n as EventFlowNode;

            var idList = nodeEdit.Content.GetNextIds();
            if (idList.Length == 0)
                continue;
            
            var list = idList.Select(s => {
                if (s == int.MinValue) return null;
                return GraphNodeHolder.GetChild(s) as EventFlowNode;
            });
            
            nodeEdit.SetupConnections(list.ToList());
        }

        // Create entry points to the graph
        foreach (var entry in Graph.EntryPoints)
        {
            var editNode = GraphNodeHolder.GetNode<EventFlowNode>(entry.Value.Id.ToString());
            if (!IsInstanceValid(editNode))
            {
                GD.PushError("Failed to initilize " + entry.Key + " entry point");
                return;
            }

            var entryEdit = SceneCreator<EventFlowNode>.Create();
            GraphNodeHolder.AddChild(entryEdit);
            entryEdit.InitContent(entry.Key, editNode);
        }
    }

    public void OpenFile(SarcFile sarc, string name)
    {
        Graph = sarc.GetFileEventFlow(name, new ProjectSmoEventFlowFactory());
        InitEditor();
    }

    #endregion

    #region Signals

    private void OnVisiblityChanged()
    {
        GraphCanvas.Visible = Visible;
        BackgroundCanvas.Visible = Visible;
    }

    #endregion
}
