using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

using MoonFlow.Project;

using Nindot;
using Nindot.Al.EventFlow;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.Scene.EditorEvent;

// ##################################################################### //
// ######### Warning! Ye headin into le land of spaghetti! Our ######### //
// ####### boats been shipwrecked on this baron island for years, ###### //
// ####### and even we couldn't clean up this mess! Proceed with ####### //
// ######################### caution, traveler. ######################## //
// ##################################################################### //

public partial class EventFlowNodeMessageBalloon : EventFlowNodeCommon
{
	[Export, ExportGroup("Internal References")]
	private TextEdit TextMessagePreview;
	[Export]
	private Label LabelTextSource;

	[Export]
	private VBoxContainer MessageResolverConfig;

	[Export]
	private CheckBox IsTalkBalloon;
	[Export]
	private CheckBox IsIconBalloon;
	[Export]
	private CheckBox IsMapUnit;
	[Export]
	private CheckBox IsMultiDivide;
	[Export]
	private CheckBox IsTutorial;

	[Flags]
	public enum NameFlags
	{
		NONE = 0,
		TALK_BALLOON = 1 << 0, // Mutually exclusive (aside from MULTI_DIVIDE)
		ICON_BALLOON = 1 << 1, // Mutually exclusive (aside from MULTI_DIVIDE)
		MULTI_DIVIDE = 1 << 2, // Always toggle-able regardless of other bits
		MAP_UNIT = 1 << 3, // Cannot be set if TUTORIAL is set
		TUTORIAL = 1 << 4, // Cannot be set if MAP_UNIT is set
	}

	private NameFlags _flags = NameFlags.NONE;
	public NameFlags Flags
	{
		get { return _flags; }
		set
		{
			if ((value & NameFlags.TALK_BALLOON) != 0)
			{
				_flags = NameFlags.TALK_BALLOON | (value & NameFlags.MULTI_DIVIDE);
				Content.Name = "MessageTalkBalloon";

				if ((_flags & NameFlags.MULTI_DIVIDE) != 0)
					Content.Name += "MultiDevide";
				
				return;
			}

			if ((value & NameFlags.ICON_BALLOON) != 0)
			{
				_flags = NameFlags.ICON_BALLOON | (value & NameFlags.MULTI_DIVIDE);
				Content.Name = "IconBalloon";

				if ((_flags & NameFlags.MULTI_DIVIDE) != 0)
					Content.Name += "MultiDevide";

				return;
			}

			if ((value & NameFlags.MAP_UNIT) != 0)
				value &= ~NameFlags.TUTORIAL;
			
			if ((value & NameFlags.TUTORIAL) != 0)
				value &= ~NameFlags.MAP_UNIT;

			_flags = value;

			if (Content != null)
			{
				var n = "MessageBalloon";
				if ((value & NameFlags.MULTI_DIVIDE) != 0) n += "MultiDevide";
				if ((value & NameFlags.MAP_UNIT) != 0) n += "MapUnit";
				if ((value & NameFlags.TUTORIAL) != 0) n += "Tutorial";

				Content.Name = n;
			}
		}
	}

	public override void InitContent(Nindot.Al.EventFlow.Node content, Graph graph)
	{
		base.InitContent(content, graph);

		// Remove name option button, this node's name should only be configured through flag picker
		NameOptionButton.QueueFree();

		// Convert node name to flags
		var name = Content.Name;

		Flags = NameFlags.NONE;
		if (name.Contains("TalkBalloon")) Flags |= NameFlags.TALK_BALLOON;
		if (name == "IconBalloon") Flags |= NameFlags.ICON_BALLOON;

		if (Flags == NameFlags.NONE)
		{
			if (name.Contains("MapUnit")) Flags |= NameFlags.MAP_UNIT;
			if (name.Contains("MultiDevide")) Flags |= NameFlags.MULTI_DIVIDE;
			if (name.Contains("Tutorial")) Flags |= NameFlags.TUTORIAL;
		}

		if (name != Content.Name)
			throw new Exception(string.Format("Name mismatch! {0} - {1}", name, Content.Name));

		if (IsSupportMessageResolver())
			if (!Content.TryGetParam("Text", out NodeMessageResolverData _))
				Content.TrySetParamMessageData("Text", new NodeMessageResolverData());

		OnToggleMultiDivide((Flags & NameFlags.MULTI_DIVIDE) != 0);
	}

	#region Signals

	private void OnToggleTalkBalloon(bool state)
	{
		if (state) Flags |= NameFlags.TALK_BALLOON;
		else Flags &= ~NameFlags.TALK_BALLOON;
		UpdateButtonStates();
		SetNodeModified();
	}
	private void OnToggleMapUnit(bool state)
	{
		if (state) Flags |= NameFlags.MAP_UNIT;
		else Flags &= ~NameFlags.MAP_UNIT;
		UpdateButtonStates();
		SetNodeModified();
	}
	private void OnToggleTutorial(bool state)
	{
		if (state) Flags |= NameFlags.TUTORIAL;
		else Flags &= ~NameFlags.TUTORIAL;
		UpdateButtonStates();
		SetNodeModified();
	}
	private void OnToggleIconBalloon(bool state)
	{
		if (state) Flags |= NameFlags.ICON_BALLOON;
		else Flags &= ~NameFlags.ICON_BALLOON;
		UpdateButtonStates();
		SetNodeModified();
	}

	private void OnToggleMultiDivide(bool state)
	{
		var oldFlags = Flags;
		if (state) Flags |= NameFlags.MULTI_DIVIDE;
		else Flags &= ~NameFlags.MULTI_DIVIDE;
		UpdateButtonStates();

		if (oldFlags != Flags)
		{
			SetNodeModified();
		}

		var portFirst = PortOutList.GetChildren().First() as PortOut;

		// Update outgoing connections
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

		var newList = new EventFlowNodeBase[1];
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
		if (arc.EndsWith(".szs"))
			arc = arc[..arc.Find(".szs")];

		if (file.EndsWith(".msbt"))
			file = file[..file.Find(".msbt")];

		Content.TrySetParam("Text", new NodeMessageResolverData(arc, file, label));
		SetLabelDisplayTextSource();

		SetNodeModified();
	}

	#endregion

	#region Utilities

	private void SetLabelDisplayTextSource()
	{
		Content.TryGetParam("Text", out NodeMessageResolverData msg);
		SetLabelDisplayTextSource(msg);
	}
	private void SetLabelDisplayTextSource(NodeMessageResolverData msg)
	{
		if (!IsContainMessageResolver())
		{
			LabelTextSource.Modulate = Colors.Crimson;
			LabelTextSource.Text = Tr("EVENT_FLOW_NODE_MESSAGE_TALK_SOURCE_PLACEHOLDER");
			return;
		}

		LabelTextSource.Modulate = Colors.LightSkyBlue;
		LabelTextSource.Text = msg.LabelName;

		// Setup preview box
		var holder = ProjectManager.GetMSBTArchives();
		SarcFile arc = holder.GetArchiveByFileName(msg.MessageArchive);
		var msbt = arc.GetFileMSBT(msg.MessageFile + ".msbt", new MsbtElementFactoryProjectSmo());

		var txt = msbt.GetEntry(msg.LabelName);
		TextMessagePreview.Text = txt.GetRawText(true);
	}

	private void UpdateButtonStates()
	{
		IsTalkBalloon.SetPressedNoSignal((Flags & NameFlags.TALK_BALLOON) != 0);
		IsIconBalloon.SetPressedNoSignal((Flags & NameFlags.ICON_BALLOON) != 0);
		IsMapUnit.SetPressedNoSignal((Flags & NameFlags.MAP_UNIT) != 0);
		IsMultiDivide.SetPressedNoSignal((Flags & NameFlags.MULTI_DIVIDE) != 0);
		IsTutorial.SetPressedNoSignal((Flags & NameFlags.TUTORIAL) != 0);

		bool isDisable = (Flags & NameFlags.TALK_BALLOON) != 0 || (Flags & NameFlags.ICON_BALLOON) != 0;

		IsMapUnit.Disabled = isDisable || (Flags & NameFlags.TUTORIAL) != 0;
		IsTutorial.Disabled = isDisable || (Flags & NameFlags.MAP_UNIT) != 0;

		IsTalkBalloon.Disabled = (Flags & NameFlags.ICON_BALLOON) != 0;
		IsIconBalloon.Disabled = (Flags & NameFlags.TALK_BALLOON) != 0;

		MessageResolverConfig.Visible = IsSupportMessageResolver();

		if ((Flags & NameFlags.MULTI_DIVIDE) != 0)
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

		var labelName = GetNode<Label>("%Label_Name");
		labelName.Text = Content.Name;
		
		SetLabelDisplayTextSource();
		DrawDebugLabel();
	}

	private bool IsSupportMessageResolver()
	{
		return ((Flags & NameFlags.TALK_BALLOON) == 0) &&
			((Flags & NameFlags.ICON_BALLOON) == 0) &&
			((Flags & NameFlags.MAP_UNIT) == 0) &&
			((Flags & NameFlags.TUTORIAL) == 0);
	}
	private bool IsContainMessageResolver()
	{
		if (!Content.TryGetParam("Text", out NodeMessageResolverData msg))
			return false;

		return msg.MessageArchive != string.Empty && msg.MessageFile != string.Empty && msg.LabelName != string.Empty;
	}

	#endregion
}
