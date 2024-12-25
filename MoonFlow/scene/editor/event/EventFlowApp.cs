using Godot;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Nindot;
using Nindot.Al.EventFlow;
using Nindot.Al.EventFlow.Smo;

using MoonFlow.Async;
using MoonFlow.Scene.Main;
using MoonFlow.Project;

namespace MoonFlow.Scene.EditorEvent;

[ScenePath("res://scene/editor/event/event_flow_app.tscn")]
[Icon("res://asset/app/icon/eventflow.png")]
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
    public NodeHolder GraphNodeHolder { get; private set; } = null;

    // ~~~~~~~~~~~~~~~~ State ~~~~~~~~~~~~~~~~ //

    public bool IsInitCompleted { get; private set; } = false;

    // ~~~~~~~~~~~~~~~ Signals ~~~~~~~~~~~~~~~ //

    [Signal]
    public delegate void EntryPointListModifiedEventHandler(string oldName, string name);
    [Signal]
    public delegate void FileOpenCompleteEventHandler();

    #endregion

    #region Initilization

    public static EventFlowApp OpenApp(SarcFile arc, string key)
	{
		var editor = SceneCreator<EventFlowApp>.Create();
		editor.SetUniqueIdentifier(arc.Name + key);
		ProjectManager.SceneRoot.NodeApps.AddChild(editor);

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
        foreach (var child in GraphNodeHolder.GetChildren())
        {
            GraphNodeHolder.RemoveChild(child);
            child.QueueFree();
        }

        await InitNodeList();
        InitEntryPointNodes();

        // If this is the first opening of this file, auto-arrange all nodes
        if (Metadata.IsFirstOpen)
        {
            Metadata.IsFirstOpen = false;
            GraphNodeHolder.ArrangeAllNodes();
        }

        IsInitCompleted = true;
        EmitSignal(SignalName.FileOpenComplete);
    }

    private async Task InitNodeList()
    {
        // Get factory function
        var factory = typeof(EventFlowNodeFactory).GetMethod("Create");

        // Initilize nodes from the Graph data container
        foreach (var node in Graph.Nodes.Values)
            InitNode(node, factory);

        await ToSignal(Engine.GetMainLoop(), "process_frame");

        // Setup node port connections
        foreach (var n in GraphNodeHolder.GetChildren())
            InitNodeConnections(n);

        await ToSignal(Engine.GetMainLoop(), "process_frame");
    }

    private void InitEntryPointNodes()
    {
        foreach (var entry in Graph.EntryPoints)
        {
            var id = entry.Value?.Id.ToString();
            EventFlowNodeCommon target = null;

            if (GraphNodeHolder.HasNode(id))
                target = GraphNodeHolder.GetNode<EventFlowNodeCommon>(id);

            // Create node
            var entryEdit = SceneCreator<EventFlowEntryPoint>.Create();
            GraphNodeHolder.AddChild(entryEdit);

            // Init main content from event flow graph byml
            entryEdit.InitContent(entry.Key, Graph, target);

            // Setup metadata access (Node position, comments, and other additional info)
            Metadata.EntryPoints.TryGetValue(entry.Key, out NodeMetadata data);
            entryEdit.InitContentMetadata(Metadata, data);
        }

        EmitSignal(SignalName.EntryPointListModified, "", "");
    }

    private EventFlowNodeCommon InitNode(Nindot.Al.EventFlow.Node node, MethodInfo factory)
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
        Metadata.Nodes.TryGetValue(node.Id, out NodeMetadata data);
        nodeEdit.InitContentMetadata(Metadata, data);

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

        var list = idList.Select(s =>
        {
            if (s == int.MinValue) return null;
            return GraphNodeHolder.FindChild(s.ToString(), true, false) as EventFlowNodeCommon;
        });

        nodeEdit.SetupConnections(list.ToList());
    }

    #endregion

    #region Read & Write

    public void OpenFile(SarcFile sarc, string name)
    {
        IsInitCompleted = false;

        Graph = sarc.GetFileEventFlow(name, new ProjectSmoEventFlowFactory());
        MetadataHolder = GraphMetaHolder.Create(Graph);

        Metadata.ArchiveName = sarc.Name;
        Metadata.FileName = name;

        var taskbar = name.TrimSuffix(".byml");
        AppTaskbarTitle = string.Format("{0} ({1})", taskbar, sarc.Name);

        InitEditor();
    }

    private async void SaveFileInternal(bool isRequireFocus) { await SaveFile(isRequireFocus); }
    public override async Task SaveFile(bool isRequireFocus)
    {
        if (!AppIsFocused() && isRequireFocus)
            return;
        
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

    #region Backend Util

    public EventFlowNodeCommon InjectNewNode(Nindot.Al.EventFlow.Node node)
    {
        var factory = typeof(EventFlowNodeFactory).GetMethod("Create");
        return InitNode(node, factory);
    }

    public void InjectNodeConnections(EventFlowNodeCommon node)
    {
        InitNodeConnections(node);
    }

    #endregion

    #region Signals

    public override string GetUniqueIdentifier(string input)
	{
		return "EVENTFLOW_" + input;
	}

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

    #endregion
}
