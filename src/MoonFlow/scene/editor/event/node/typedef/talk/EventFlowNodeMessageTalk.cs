using Godot;
using Nindot.Al.EventFlow;
using System;

namespace MoonFlow.Scene.EditorEvent;

public partial class EventFlowNodeMessageTalk : EventFlowNodeCommon
{
	[Export, ExportGroup("Internal References")]
	private CheckBox ButtonIsMapUnit;
	[Export]
	private TextEdit TextMessagePreview;
	[Export]
	private Label LabelTextSource;

	public override void InitContent(Nindot.Al.EventFlow.Node content, Graph graph)
	{
		base.InitContent(content, graph);

		if (Content.Name.Contains("MapUnit"))
		{
			ButtonIsMapUnit.ButtonPressed = true;
			return;
		}

		ButtonIsMapUnit.ButtonPressed = false;

		if (!Content.TryGetParam("Text", out NodeMessageResolverData msg))
		{
			Content.TrySetParamMessageData("Text", new NodeMessageResolverData());
			msg = (NodeMessageResolverData)Content.Params["Text"];
		}

		SetLabelTextSource(msg);
	}

	#region Signals

	protected override void OnNodeNameModified()
	{
		var value = Content.Name.Contains("MapUnit");

		if (ButtonIsMapUnit.ButtonPressed != value)
			ButtonIsMapUnit.ButtonPressed = value;

		ButtonIsMapUnit.Disabled = !IsSupportMapUnit();
		SetLabelTextSource();
	}

	private void OnMapUnitToggled(bool state)
	{
		if (!IsSupportMapUnit())
		{
			ButtonIsMapUnit.ButtonPressed = false;
			return;
		}

		var con = Content.Name.Contains("MapUnit");

		if (!con && state)
			OnSetName(Content.Name + "MapUnit");
		else if (con && !state)
			OnSetName(Content.Name[..Content.Name.Find("MapUnit")]);
		
		SetLabelTextSource();
	}

	#endregion

	#region Utilities

	private void SetLabelTextSource()
	{
		Content.TryGetParam("Text", out NodeMessageResolverData msg);
		SetLabelTextSource(msg);
	}
	private void SetLabelTextSource(NodeMessageResolverData msg)
	{
		if (IsContainMessageResolver())
		{
			LabelTextSource.Modulate = Colors.LightSkyBlue;
			LabelTextSource.Text = string.Format("{0}/{1}/{2}", msg.MessageArchive, msg.MessageFile, msg.LabelName);
			return;
		}

		LabelTextSource.Modulate = Colors.Crimson;
		LabelTextSource.Text = Tr("EVENT_FLOW_NODE_MESSAGE_TALK_SOURCE_PLACEHOLDER");
	}

	private bool IsSupportMapUnit()
	{
		var s = Content.Name;
		return s == "MessageTalk" || s == "MessageTalkMapUnit" || s == "MessageTalkDemo" || s == "MessageTalkDemoMapUnit";
	}
	private bool IsContainMessageResolver()
	{
		if (!Content.TryGetParam("Text", out NodeMessageResolverData msg))
			return false;
		
		return msg.MessageArchive == string.Empty || msg.MessageFile == string.Empty || msg.LabelName == string.Empty;
	}

	#endregion
}
