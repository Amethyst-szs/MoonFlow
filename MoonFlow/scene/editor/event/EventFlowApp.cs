using Godot;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using Nindot;
using Nindot.Al.EventFlow;
using Nindot.Al.EventFlow.Smo;

using MoonFlow.Async;
using MoonFlow.Scene.Main;
using MoonFlow.Project;

namespace MoonFlow.Scene.EditorEvent;

[SceneUid("uid://myiyk7kteiqb"), Icon("res://asset/app/icon/eventflow.png")]
public partial class EventFlowApp : AppScene
{
    #region Properties

    // ~~~~~~~~~~~ Event Flow Graph ~~~~~~~~~~ //

    public SarcEventFlowGraph Graph { get; private set; } = null;
    public GraphMetadataFile MetadataHolder { get; private set; } = null;
    public GraphMetaBucketCommon Metadata { get { return MetadataHolder.Data; } }

    // ~~~~~~~~~ Internal References ~~~~~~~~~ //

    [Export, ExportGroup("Internal References")]
    public GraphCanvas GraphCanvas { get; private set; } = null;
    [Export]
    private CanvasLayer BackgroundCanvas = null;
    [Export]
    public NodeHolder GraphNodeHolder { get; private set; } = null;
    [Export]
    public Node2D GraphBlockHolder { get; private set; } = null;
    [Export]
    public PopupEventMetadata PopupMetadata { get; private set; } = null;

    // ~~~~~~~~~~~~~~~~ State ~~~~~~~~~~~~~~~~ //

    public bool IsInitCompleted { get; private set; } = false;

    // ~~~~~~~~~~~~~~~ Signals ~~~~~~~~~~~~~~~ //

    [Signal]
    public delegate void EntryPointListModifiedEventHandler();
    [Signal]
    public delegate void FileOpenCompleteEventHandler();

    #endregion

    #region Initilization

    public override void _EnterTree()
    {
        // Spawn inject menu if it doesn't already exist
        var scene = ProjectManager.SceneRoot;
        if (scene.HasNode(PopupInjectGraphNode.DefaultNodeName))
            return;

        var popup = SceneCreator<PopupInjectGraphNode>.Create();
        popup.Name = PopupInjectGraphNode.DefaultNodeName;
        scene.AddChild(popup);
    }

    public static EventFlowApp OpenApp(SarcFile arc, string key)
    {
        var editor = AppSceneServer.CreateApp<EventFlowApp>(arc.Name + key);
        editor.OpenFile(arc, key);

        return editor;
    }

    protected override void AppInit()
    {
        // Connect to signal events
        VisibilityChanged += OnVisiblityChanged;

        Scene.NodeHeader.Connect(Header.SignalName.ButtonSave, Callable.From(new Action<bool>(SaveFileInternal)));
    }

    public async void InitEditor()
    {
        IsInitCompleted = false;

        // Destroy current contents of editor, if any exist
        GraphNodeHolder.QueueFreeAllChildren();

        PrepareEntryPointMetadata();

        await InitNodeList();
        InitEntryPointNodes();

        // Create metadata-only block objects
        InitBlockList();

        // Cleanup metadata in-case it contains extra unused data in node list
        Metadata.CleanupStaleMetadata(Graph);

        // If this is the first opening of this file, auto-arrange all nodes
        if (Metadata.IsFirstOpen)
        {
            Metadata.IsFirstOpen = false;
            await GraphNodeHolder.ArrangeAllNodes();
        }

        IsInitCompleted = true;
        EmitSignal(SignalName.FileOpenComplete);
    }

    private void PrepareEntryPointMetadata()
    {
        // Create missing entry point metadata keys
        foreach (var entry in Graph.EntryPoints)
        {
            var pointMeta = Metadata.GetEntryPointByName(entry.Key);

            // If the metadata has not registered this entry point, create an entry
            if (pointMeta == null)
            {
                pointMeta = new GraphMetaBucketEntryPoint
                {
                    Name = entry.Key
                };

                pointMeta.TryAssignUid();
                Metadata.EntryPoints.Add(pointMeta.Uid, pointMeta);
            }
        }

        // Remove unused entry point metadata keys
        foreach (var entry in Metadata.EntryPoints.Values)
        {
            if (!Graph.EntryPoints.ContainsKey(entry.Name))
                Metadata.EntryPoints.Remove(entry.Uid);
        }
    }

    private async Task InitNodeList()
    {
        // Get factory function
        var factory = typeof(EventFlowNodeFactory).GetMethod("Create");

        // Initilize nodes from the Graph data container
        foreach (var node in Graph.Nodes.Values)
            InitNode(node, factory);

        await Extension.WaitProcessFrame(this);

        // Setup node port connections
        foreach (var n in GraphNodeHolder.GetChildren())
            InitNodeConnections(n);

        await Extension.WaitProcessFrame(this);
    }

    private void InitEntryPointNodes()
    {
        foreach (var entry in Graph.EntryPoints)
            InitEntryPoint(entry);

        EmitSignalEntryPointListModified();
    }

    private EventFlowNodeCommon InitNode(Nindot.Al.EventFlow.Node node, MethodInfo factory, GraphMetaBucketNode meta = null)
    {
        if (node.Id == int.MinValue)
            throw new EventFlowException("Node initilized without an Id!");

        // Create node
        var factoryMethod = factory.MakeGenericMethod(node.GetType());
        var nodeEdit = factoryMethod.Invoke(null, null) as EventFlowNodeCommon;
        GraphNodeHolder.AddChild(nodeEdit);

        // Init main content from event flow graph byml
        nodeEdit.InitContent(node, Graph);

        // Setup metadata access (Node position, comments, and other additional info)
        if (meta == null)
            Metadata.Nodes.TryGetValue(node.Id, out meta);
        else // If the metadata doesn't know about a node of this ID already, register that here
            Metadata.Nodes[node.Id] = meta;

        nodeEdit.InitContentMetadata(Metadata, meta);
        return nodeEdit;
    }

    private void InitNodeConnections(Godot.Node node)
    {
        var t = node.GetType();
        if (t == typeof(EventFlowEntryPoint) || !t.IsSubclassOf(typeof(EventFlowNodeBase)))
            return;

        var nodeEdit = node as EventFlowNodeCommon;

        var idList = nodeEdit.Content.GetNextIds();
        if (idList.Length == 0)
            return;

        var list = idList.Select(id =>
        {
            if (id == int.MinValue) return null;
            return GraphNodeHolder.FindChild(id.ToString(), false, false) as EventFlowNodeCommon;
        });

        nodeEdit.SetupConnections(list.ToList());
    }

    private EventFlowEntryPoint InitEntryPoint(KeyValuePair<string, Nindot.Al.EventFlow.Node> entry)
    {
        var id = entry.Value?.Id.ToString();
        EventFlowNodeCommon target = null;

        if (GraphNodeHolder.HasNode(id))
            target = GraphNodeHolder.GetNode<EventFlowNodeCommon>(id);

        // Create node
        var entryEdit = SceneCreator<EventFlowEntryPoint>.Create();
        GraphNodeHolder.AddChild(entryEdit);

        // Fetch metadata for node
        var pointMeta = Metadata.GetEntryPointByName(entry.Key);

        // If the lookup failed, create a new metadata entry for this entry point
        if (pointMeta == null)
        {
            pointMeta = new GraphMetaBucketEntryPoint
            {
                Name = entry.Key
            };

            pointMeta.TryAssignUid();
            Metadata.EntryPoints.Add(pointMeta.Uid, pointMeta);
        }

        entryEdit.SetupEntryPointMetadata(Metadata, pointMeta);

        // Init main content from event flow graph byml
        entryEdit.InitContent(entry.Key, Graph, target);

        return entryEdit;
    }

    private void InitBlockList()
    {
        foreach (var block in Metadata.Blocks.Keys)
            CreateBlock(block);
    }

    #endregion

    #region Read & Write

    public void OpenFile(SarcFile sarc, string name)
    {
        IsInitCompleted = false;

        Graph = sarc.GetFileEventFlow(name, new ProjectSmoEventFlowFactory());
        MetadataHolder = GraphMetadataFile.Create(Graph, ProjectManager.GetPath());

        Metadata.ArchiveName = sarc.Name;
        Metadata.FileName = name;

        var taskbar = name.TrimSuffix(".byml");
        AppTaskbarTitle = string.Format("{0} ({1})", taskbar, sarc.Name);

        InitEditor();
    }

    private async void SaveFileInternal(bool isRequireFocus) { await AppSaveContent(isRequireFocus); }
    protected override void TaskWriteAppSaveContent(AsyncDisplay display)
    {
        // Write graph event data
        display.UpdateProgress(0, 2);
        Graph.WriteArchive();

        // Write metadata holder
        display.UpdateProgress(1, 2);
        Metadata.CleanupStaleMetadata(Graph);
        MetadataHolder.WriteFile();

        // Reset flag
        display.UpdateProgress(2, 2);
        IsModified = false;
    }

    #endregion

    #region Backend Util

    public EventFlowNodeCommon InjectNewNode(Nindot.Al.EventFlow.Node node, GraphMetaBucketNode meta = null)
    {
        var factory = typeof(EventFlowNodeFactory).GetMethod("Create");
        return InitNode(node, factory, meta);
    }

    public void InjectNodeConnections(EventFlowNodeCommon node)
    {
        InitNodeConnections(node);
    }

    public EventFlowEntryPoint InjectNewEntryPoint(string name, EventFlowNodeCommon connection = null)
    {
        var pair = new KeyValuePair<string, Nindot.Al.EventFlow.Node>(name, connection?.Content);
        var entry = InitEntryPoint(pair);

        EmitSignalEntryPointListModified();

        return entry;
    }

    public EventBlockPanel CreateBlock() { return CreateBlock(Metadata.CreateBlockId()); }
    public EventBlockPanel CreateBlock(string id)
    {
        var block = SceneCreator<EventBlockPanel>.Create();
        block.InitPanel(Metadata, id);
        GraphBlockHolder.AddChild(block);

        block.Connect(EventBlockPanel.SignalName.BlockModified, Callable.From(OnFileModified));

        OnFileModified();
        return block;
    }

    public override string GetUniqueIdentifier(string input)
    {
        return "EVENTFLOW_" + input;
    }

    #endregion

    #region Signals

    private void OnVisiblityChanged()
    {
        GraphCanvas.Visible = Visible;
        BackgroundCanvas.Visible = Visible;
    }

    private void OnFileModified()
    {
        if (IsInitCompleted)
            IsModified = true;
    }

    public void OnMetadataEditRequest(EventFlowNodeCommon source)
    {
        OnFileModified();
        PopupMetadata.SetupPopup(source);
    }

    private static void OnSliderGridSnapSizeChanged(float size)
    {
        EngineSettings.SetSetting("moonflow/event_graph/grid_snap_size", size);
        EngineSettings.Save();
    }

    #endregion
}
