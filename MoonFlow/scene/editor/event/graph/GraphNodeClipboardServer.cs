using System;
using System.Collections.Generic;
using System.Linq;
using Nindot.Al.EventFlow;

namespace MoonFlow.Scene.EditorEvent;

public static class GraphNodeClipboardServer
{
    private static readonly List<Node> Nodes = [];
    private static readonly Dictionary<int, Godot.Vector2> NodePositions = [];

    public static void Copy(IList<EventFlowNodeCommon> cNodes)
    {
        if (cNodes.Count == 0)
            return;
        
        // Clear current clipboard contents
        Nodes.Clear();
        NodePositions.Clear();

        // Create a list of all node positions
        foreach (var node in cNodes)
            NodePositions.Add(node.Content.Id, node.GlobalPosition);

        // Push all node positions to be based on 0,0 origin
        var lowest = new Godot.Vector2(NodePositions.Select(p => p.Value.X).Min(),
            NodePositions.Select(p => p.Value.Y).Min());
        
        foreach (var pos in NodePositions)
            NodePositions[pos.Key] = pos.Value - lowest;

        // Setup lists
        var cNodesSrc = cNodes.Select(n => n.Content).ToList();

        // Clone all provided nodes into internal list
        CloneIntoClipboard(cNodesSrc);

        var ctx = Nodes.Select(n => n.Id).ToList();

        // Remove any connections to nodes that aren't a part of this clipboard selection
        foreach (var node in Nodes)
        {
            var targets = node.GetNextIds();
            for (int i = 0; i < targets.Length; i++)
            {
                if (!ctx.Contains(targets[i]))
                    node.RemoveNextNode(i);
            }
        }

        // Reassign node IDs
        foreach (var node in Nodes)
            ctx.Add(ReassignIdsInList(Nodes, node, ctx));
    }

    public static async void Paste(GraphCanvas context)
    {
        if (Nodes.Count == 0)
            return;
        
        // Get list of ids from the paste context
        var ctxNodes = context.Parent.GraphNodeHolder.GetChildren();
        var ctxIdList = new List<int>();

        foreach (var node in ctxNodes)
        {
            var t = node.GetType();
            if (!t.IsSubclassOf(typeof(EventFlowNodeBase)) || t == typeof(EventFlowEntryPoint))
                continue;

            ctxIdList.Add(((EventFlowNodeCommon)node).Content.Id);
        }

        // Reassign clipboard node ids to not conflict with any ids in context
        foreach (var node in Nodes)
            ctxIdList.Add(ReassignIdsInList(Nodes, node, ctxIdList));

        // Insert new data into graph data
        var graph = context.Graph;

        foreach (var node in Nodes)
            graph.AddNode(node);

        // Inject new nodes into graph
        var nodeEditors = new List<EventFlowNodeCommon>();

        foreach (var node in Nodes)
            nodeEditors.Add(context.Parent.InjectNewNode(node));

        await context.ToSignal(Godot.Engine.GetMainLoop(), "process_frame");

        // Setup connections and positions between newly injected nodes
        var factor = Godot.Vector2.One / context.Scale;
        var offset = context.Offset * factor;
        offset -= (Godot.Vector2)context.GetWindow().Size / 2.5F * factor;

        foreach (var node in nodeEditors)
        {
            context.Parent.InjectNodeConnections(node);

            if (NodePositions.TryGetValue(node.Content.Id, out Godot.Vector2 vec))
                node.SetPosition(vec - offset);
        }

        // Re-clone internal clipboard nodes in case user pastes same clipboard again
        var list = Nodes.ToArray();
        Nodes.Clear();

        CloneIntoClipboard(list);
    }

    private static void CloneIntoClipboard(IList<Node> cNodes)
    {
        for (int i = 0; i < cNodes.Count; i++)
        {
            var source = cNodes[i];
            var type = source.GetType();

            var copyFactory = type.GetMethod("Clone").MakeGenericMethod(type);
            var copy = (Node)copyFactory.Invoke(source, []);

            Nodes.Add(copy);
        }
    }

    private static int ReassignIdsInList(IList<Node> list, Node node, IList<int> context)
    {
        // Find first valid ID that isn't used in the context
        int? newId = Enumerable.Range(0, int.MaxValue).Except(context).FirstOrDefault();
        if (newId == null)
            throw new Exception("Failed to create new node id");

        // Copy down old ID and set node ID
        var oldId = node.Id;

        node.SetIdUnsafe((int)newId);

        // Iterate through list and reassign all references to the old node ID with the new ID
        foreach (var cmp in list)
        {
            var targets = cmp.GetNextIds();
            if (!targets.Contains(oldId))
                continue;

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == oldId)
                    cmp.TrySetNextNode(node, i);
            }
        }

        // Update NodePositions list to match new id
        if (NodePositions.TryGetValue(oldId, out Godot.Vector2 pos))
        {
            NodePositions.Remove(oldId);
            NodePositions.Add(node.Id, pos);
        }

        return (int)newId;
    }
}