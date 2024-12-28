using Godot;
using Nindot.Al.EventFlow;
using System;
using System.Collections.Generic;

namespace MoonFlow.Scene.EditorEvent;

public partial class EventFlowNodeChoice : EventFlowNodeMessageTalk
{
	[Export]
	private int MaxOutgoingEdges = 2;

    public override void InitContent(Nindot.Al.EventFlow.Node content, Graph graph)
    {
        base.InitContent(content, graph);

		// Require four outgoing ports
		content.CaseEventList.TryIncreaseCaseListSize(MaxOutgoingEdges);

		while (PortOutList.GetChildCount() < MaxOutgoingEdges)
			CreatePortOut();
		
		Array.Resize(ref Connections, MaxOutgoingEdges);
    }

    public override void SetupConnections(List<EventFlowNodeCommon> list)
    {
        base.SetupConnections(list);
		Array.Resize(ref Connections, MaxOutgoingEdges);
    }

    protected override PortOut CreatePortOut()
    {
        var port = base.CreatePortOut();
		port.PortColor = PortColorList[port.Index];

		return port;
    }
}
