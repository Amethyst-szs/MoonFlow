using System;
using Godot;

using MoonFlow.Project;

using Nindot;
using Nindot.Al.EventFlow;
using Nindot.LMS.Msbt.TagLib.Smo;

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

		SetLabelDisplayTextSource(msg);
	}

	#region Signals

	protected override void OnNodeNameModified()
	{
		var value = Content.Name.Contains("MapUnit");

		if (ButtonIsMapUnit.ButtonPressed != value)
			ButtonIsMapUnit.ButtonPressed = value;

		ButtonIsMapUnit.Disabled = !IsSupportMapUnit();
		SetLabelDisplayTextSource();
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
		
		SetLabelDisplayTextSource();
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

	private bool IsSupportMapUnit()
	{
		var s = Content.Name;
		return s == "MessageTalk" || s == "MessageTalkMapUnit" || s == "MessageTalkDemo" || s == "MessageTalkDemoMapUnit";
	}
	private bool IsContainMessageResolver()
	{
		if (!Content.TryGetParam("Text", out NodeMessageResolverData msg))
			return false;
		
		return msg.MessageArchive != string.Empty && msg.MessageFile != string.Empty && msg.LabelName != string.Empty;
	}

	#endregion
}
