using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using Nindot.Al.EventFlow;

namespace MoonFlow.Scene.EditorEvent;

[GlobalClass]
[ScenePath("res://scene/editor/event/node/typedef/join/join.tscn")]
public partial class EventFlowNodeJoin : EventFlowNodeCommon
{
	protected Nindot.Al.EventFlow.Smo.NodeJoin NodeJoin;

    public override void InitContent(Nindot.Al.EventFlow.Node content, Graph graph)
    {
        base.InitContent(content, graph);

		PortIn.Connect(PortIn.SignalName.IncomingListModified, Callable.From(OnIncomingModified));

		NodeJoin = content as Nindot.Al.EventFlow.Smo.NodeJoin;
    }

    private void OnIncomingModified()
	{
		List<int> IdList = [];
		foreach (var con in PortIn.IncomingList)
		{
			if (con.Parent is EventFlowEntryPoint)
				continue;
			
			IdList.Add((con.Parent as EventFlowNodeCommon).Content.Id);
		}

		NodeJoin.PreIdList = IdList;
		
		DrawDebugLabel();
	}

    protected override void DrawDebugLabel()
    {
        base.DrawDebugLabel();

		string txt = "";

		if (NodeJoin != null)
		{
			txt += "PreId: ";
			foreach (var id in NodeJoin.PreIdList) txt += id.ToString() + ", ";
			txt += '\n';
		}

		DebugDataDisplay.Text += txt;
    }
}
