using Godot;
using System;

using Nindot.Al.EventFlow;
using System.Linq;

namespace MoonFlow.Scene.EditorEvent;

public partial class EventFlowNodeEventQuery : EventFlowNodeCommon
{
    public override void InitContent(Nindot.Al.EventFlow.Node content, Graph graph)
    {
        base.InitContent(content, graph);
    }

    protected override PortOut CreatePortOut()
    {
        var port = base.CreatePortOut();

		if (PortOutList.GetChildCount() <= 2)
		{
			if (port.Index == 0) port.TooltipText = port.Index.ToString() + " / false";
			else port.TooltipText = port.Index.ToString() + " / true";
		}
		else
		{
			port.TooltipText = port.Index.ToString();
		}

		return port;
    }

    private void OnButtonPressedRemovePort()
	{
		// If the port list is 2 or less, return
		if (PortOutList.GetChildCount() <= 2)
			return;
		
		// Remove connection from last port
		var lastPort = PortOutList.GetChildren().Last() as PortOut;
		var portIdx = lastPort.Index;

		lastPort.Connection = null;
		lastPort.QueueFree();

		Content.CaseEventList.CaseList.RemoveAt(portIdx);

		GenerateIdList();
	}

	private void OnButtonPressedAddPort()
	{
		CreatePortOut();
		GenerateIdList();
	}

	private void GenerateIdList()
	{
		var ids = Content.GetNextIds();
		var list = ids.Select(s =>
		{
			if (s == int.MinValue) return null;
			return Application.GraphNodeHolder.GetChild(s) as EventFlowNodeCommon;
		});
			
		SetupConnections([.. list]);
		SetNodeModified();

		DrawDebugLabel();
	}
}
