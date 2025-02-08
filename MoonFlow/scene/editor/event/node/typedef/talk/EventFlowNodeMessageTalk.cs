using System;
using Godot;

using MoonFlow.Project;
using MoonFlow.Scene.EditorMsbt;

using Nindot;
using Nindot.Al.EventFlow;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.Scene.EditorEvent;

public partial class EventFlowNodeMessageTalk : EventFlowNodeCommon
{
	[Export, ExportGroup("Internal References")]
	private CheckBox ButtonIsMapUnit;

	[Export, ExportSubgroup("Preview Tools")]
	protected MsbtPageEditor TextMessagePreview;
	[Export]
	protected Label LabelTextSource;
	[Export]
	protected Button ButtonMessageEdit;
	[Export]
	protected Button ButtonMessageRefresh;

	public override void InitContent(Nindot.Al.EventFlow.Node content, Graph graph)
	{
		base.InitContent(content, graph);

		if (Content.Name.Contains("MapUnit"))
		{
			if (ButtonIsMapUnit != null)
				ButtonIsMapUnit.ButtonPressed = true;

			return;
		}

		if (ButtonIsMapUnit != null)
			ButtonIsMapUnit.ButtonPressed = false;

		if (!Content.TryGetParam("Text", out NodeMessageResolverData msg))
		{
			if (Content.TrySetParamMessageData("Text", new NodeMessageResolverData()))
				msg = (NodeMessageResolverData)Content.Params["Text"];
		}

		if (msg != null && msg.MessageArchive != string.Empty && msg.MessageFile != string.Empty)
			SetLabelDisplayTextSource(msg);
	}

	#region Signals

	protected override void OnNodeNameModified()
	{
		if (ButtonIsMapUnit == null)
			return;

		var value = Content.Name.Contains("MapUnit");

		if (ButtonIsMapUnit.ButtonPressed != value)
			ButtonIsMapUnit.ButtonPressed = value;

		ButtonIsMapUnit.Disabled = !IsSupportMapUnit();
		SetLabelDisplayTextSource();
		SetNodeModified();
	}

	private void OnMapUnitToggled(bool state)
	{
		if (ButtonIsMapUnit == null)
			return;

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
		SetNodeModified();
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

	protected void OnTextPreviewEditCurrent()
	{
		if (!Content.TryGetParam("Text", out NodeMessageResolverData msg))
			return;
		
		if (!IsContainMessageResolver())
			return;
		
		_ = AppSceneServer.CreateOrOpenMsbtLabel(msg);
	}
	protected void OnTextPreviewRefreshCurrent()
	{
		SetLabelDisplayTextSource();
	}

	#endregion

	#region Utilities

	protected void SetLabelDisplayTextSource()
	{
		Content.TryGetParam("Text", out NodeMessageResolverData msg);
		SetLabelDisplayTextSource(msg);
	}
	protected void SetLabelDisplayTextSource(NodeMessageResolverData msg)
	{
		if (!IsContainMessageResolver())
		{
			LabelTextSource.Modulate = Colors.Crimson;
			LabelTextSource.Text = Tr("EVENT_FLOW_NODE_MESSAGE_TALK_SOURCE_PLACEHOLDER");

			ButtonMessageEdit.Disabled = true;
			ButtonMessageRefresh.Disabled = true;
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

		var entry = msbt.GetEntry(msg.LabelName);

		switch(entry.Pages.Count)
		{
			case 0:
				TextMessagePreview.Text = "";
				break;
			case 1:
				TextMessagePreview.Init(null, entry.Pages[0]);
				break;
			default:
				TextMessagePreview.Init(null, entry.Pages[0]);
				break;
		}

		ButtonMessageEdit.Disabled = false;
		ButtonMessageRefresh.Disabled = false;
	}

	protected bool IsSupportMapUnit()
	{
		var s = Content.Name;
		return s == "MessageTalk" || s == "MessageTalkMapUnit" || s == "MessageTalkDemo" || s == "MessageTalkDemoMapUnit";
	}
	protected virtual bool IsContainMessageResolver()
	{
		if (!Content.TryGetParam("Text", out NodeMessageResolverData msg))
			return false;

		return msg.MessageArchive != string.Empty && msg.MessageFile != string.Empty && msg.LabelName != string.Empty;
	}

	#endregion
}
