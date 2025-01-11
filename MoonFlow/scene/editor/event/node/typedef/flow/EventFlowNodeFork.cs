using Godot;
using System.Linq;

using Nindot.Al.EventFlow;
using Nindot.Al.EventFlow.Smo;

namespace MoonFlow.Scene.EditorEvent;

[GlobalClass]
[ScenePath("res://scene/editor/event/node/typedef/fork/fork.tscn")]
public partial class EventFlowNodeFork : EventFlowNodeCommon
{
	private NodeFork ForkNode;

    public override void InitContent(Nindot.Al.EventFlow.Node content, Graph graph)
    {
        base.InitContent(content, graph);

		ForkNode = content as NodeFork;
    }

    private void OnButtonPressedRemovePort()
	{
		// If the port list is 2 or less, return
		if (PortOutList.GetChildCount() <= 2)
			return;
		
		// Remove connection from last port
		var lastPort = PortOutList.GetChildren().Last() as PortOut;
		lastPort.Connection = null;

		ForkNode.NextIdList.RemoveAt(lastPort.Index);

		lastPort.QueueFree();

		// Regenerate id list
		var ids = Content.GetNextIds();
		var list = ids.Select(s =>
		{
			if (s == int.MinValue) return null;
			return Application.GraphNodeHolder.FindChild(s.ToString(), true, false) as EventFlowNodeCommon;
		});
			
		SetupConnections(list.ToList());
		SetNodeModified();

		DrawDebugLabel();
	}

	private void OnButtonPressedAddPort()
	{
		ForkNode.NextIdList.Add(int.MinValue);

		// Create new port
		var port = CreatePortOut();

		// Regenerate id list
		var ids = Content.GetNextIds();
		var list = ids.Select(s =>
		{
			if (s == int.MinValue) return null;
			return Application.GraphNodeHolder.GetChild(s) as EventFlowNodeCommon;
		});
			
		SetupConnections(list.ToList());
		SetNodeModified();

		DrawDebugLabel();
	}
}
