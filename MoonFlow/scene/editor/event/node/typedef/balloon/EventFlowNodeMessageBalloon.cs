using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

using MoonFlow.Project;
using MoonFlow.Scene.EditorWorld;

using Nindot;
using Nindot.Al.EventFlow;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.Scene.EditorEvent;

public partial class EventFlowNodeMessageBalloon : EventFlowNodeCommon
{
	[Export, ExportGroup("Internal References")]
	private TextEdit TextMessagePreview;
	[Export]
	private Label LabelTextSource;

	[Export]
	private VBoxContainer MessageResolverConfig;

	[Export]
	private OptionBalloonType OptionBalloon;
	[Export]
	private OptionSourceType OptionSource;
	[Export]
	private CheckBox IsMultiDivide;

	public override void InitContent(Nindot.Al.EventFlow.Node content, Graph graph)
	{
		base.InitContent(content, graph);

		// Remove name option button
		NameOptionButton.QueueFree();
		NameLineEdit.QueueFree();

		// Setup buttons
		OptionBalloon.SetupSelection(content.Name);
		OptionSource.SetupSelection(content.Name);
		IsMultiDivide.SetPressedNoSignal(content.Name.Contains("MultiDevide"));

		// Setup source selection
		SetLabelDisplayTextSource();
	}

	#region Signals

	private void OnBalloonTypeOrSourceModified()
	{
		var type = (OptionBalloonType.Options)OptionBalloon.Selected;
		var src = (OptionSourceType.Options)OptionSource.Selected;

		// If using the IconBalloon type, reset source to default
		if (type == OptionBalloonType.Options.IconBalloon)
		{
			src = OptionSourceType.Options.Path;
			OptionSource.Selected = (int)src;
			OptionSource.Disabled = true;
		}
		else
		{
			OptionSource.Disabled = false;
		}

		UpdateNodeName(type, src);
	}

	private void OnToggleMultiDivide(bool state)
	{
		SetNodeModified();

		// Update node name
		OnBalloonTypeOrSourceModified();

		// Update content connection list
		if (Content.Name.EndsWith("MultiDevide"))
		{
			Content.CaseEventList ??= new();
			for (int i = 0; i < Connections.Length; i++)
				Content.TrySetNextNode((Connections[i] as EventFlowNodeCommon)?.Content, i);
		}
		else
		{
			Content.CaseEventList = null;
			if (Connections.Length != 0)
			{
				var con = (Connections[0] as EventFlowNodeCommon)?.Content;
				if (con != null)
					Content.TrySetNextNode(con);
				else
					Content.RemoveNextNode();
			}
		}

		// Update outgoing port nodes
		var portFirst = PortOutList.GetChildren().First() as PortOut;

		if (state)
		{
			if (Connections.Length == 2)
				return;

			CreatePortOut();
			portFirst.PortColor = PortColorList[portFirst.Index];

			if (Connections.Length == 0)
				return;

			var listSize2 = new List<EventFlowNodeCommon>
			{
				(EventFlowNodeCommon)Connections[0],
				null
			};

			SetupConnections(listSize2);
			return;
		}

		if (Connections.Length < 2)
			return;

		// Remove connection from last port
		var lastPort = PortOutList.GetChildren().Last() as PortOut;
		lastPort.Connection = null;
		lastPort.QueueFree();

		var newList = new EventFlowNodeCommon[1];
		newList[0] = Connections[0];
		Connections = newList;

		if (Connections.Length == 1)
			Content.TrySetNextNode((Connections[0] as EventFlowNodeCommon)?.Content);

		portFirst.PortColor = DefaultPortOutColor;
		DrawDebugLabel();
	}

	private void OnSelectNewTextSource()
	{
		var popup = SceneCreator<PopupMsbtSelectEntry>.Create();
		GetTree().CurrentScene.AddChild(popup);
		popup.Popup();

		popup.Connect(PopupMsbtSelectEntry.SignalName.ItemSelected, Callable.From(
			new Action<string, string, string>(OnNewTextSourceSelectedFromPopup)
		));
	}

	private void OnNewTextSourceSelectedFromPopup(string arc, string file, string label)
	{
		// Setup text resolver
		arc = arc.RemoveFileExtension();
		file = file.RemoveFileExtension();

		Content.TrySetParam("Text", new NodeMessageResolverData(arc, file, label));
		SetLabelDisplayTextSource();

		SetNodeModified();
	}

	#endregion

	#region Utilities

	private void UpdateNodeName(OptionBalloonType.Options type, OptionSourceType.Options source)
	{
		string name = "Message";

		switch (type)
		{
			case OptionBalloonType.Options.MiniBalloon:
				name += "Balloon";
				break;
			case OptionBalloonType.Options.TalkBalloon:
				name += "TalkBalloon";
				break;
			case OptionBalloonType.Options.IconBalloon:
				name = "IconBalloon";
				break;
		}

		switch (source)
		{
			case OptionSourceType.Options.MapUnit:
				name += "MapUnit";
				break;
			case OptionSourceType.Options.Tutorial:
				name += "Tutorial";
				break;
		}

		if (IsMultiDivide.ButtonPressed) name += "MultiDevide";

		OnSetName(name);

		SetLabelDisplayTextSource();
		SetNodeModified();
	}

	private void SetLabelDisplayTextSource()
	{
		MessageResolverConfig.Visible = IsSupportMessageResolver();

		Content.TryGetParam("Text", out NodeMessageResolverData msg);

		if (!IsContainMessageResolver())
		{
			LabelTextSource.Modulate = Colors.Crimson;
			LabelTextSource.Text = Tr("EVENT_FLOW_NODE_MESSAGE_TALK_SOURCE_PLACEHOLDER");
			return;
		}

		LabelTextSource.Modulate = Colors.LightSkyBlue;
		LabelTextSource.Text = msg.LabelName;
		LabelTextSource.TooltipText = string.Format("{0}/{1}/{2}",
			msg.MessageArchive,
			msg.MessageFile,
			msg.LabelName
		);

		// Setup preview box
		var holder = ProjectManager.GetMSBTArchives();
		SarcFile arc = holder.GetArchiveByFileName(msg.MessageArchive);
		var msbt = arc.GetFileMSBT(msg.MessageFile + ".msbt", new MsbtElementFactoryProjectSmo());

		var txt = msbt.GetEntry(msg.LabelName);
		TextMessagePreview.Text = txt.GetRawText(true);
	}

	private bool IsSupportMessageResolver()
	{
		var type = (OptionBalloonType.Options)OptionBalloon.Selected;
		var src = (OptionSourceType.Options)OptionSource.Selected;

		return type == OptionBalloonType.Options.MiniBalloon && src == OptionSourceType.Options.Path;
	}

	private bool IsContainMessageResolver()
	{
		if (!Content.TryGetParam("Text", out NodeMessageResolverData msg))
			return false;

		return msg.MessageArchive != string.Empty && msg.MessageFile != string.Empty && msg.LabelName != string.Empty;
	}

	#endregion
}
