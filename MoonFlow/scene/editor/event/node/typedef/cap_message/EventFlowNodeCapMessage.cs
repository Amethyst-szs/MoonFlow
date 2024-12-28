using Godot;
using System;

using Nindot;
using Nindot.Al.EventFlow;
using Nindot.LMS.Msbt.TagLib.Smo;

using MoonFlow.Project;

namespace MoonFlow.Scene.EditorEvent;

public partial class EventFlowNodeCapMessage : EventFlowNodeMessageTalk
{
	public override void InitContent(Nindot.Al.EventFlow.Node content, Graph graph)
	{
		base.InitContent(content, graph);
		
		if (!Content.TryGetParam("Text", out NodeMessageResolverData msg))
		{
			Content.Params["Text"] = new NodeMessageResolverDataOnlyLabel();

			var label = (NodeMessageResolverDataOnlyLabel)Content.Params["Text"];
			msg = new NodeMessageResolverData("SystemMessage", "CapMessage", label.LabelName);
		}

		SetLabelDisplayTextSource(new NodeMessageResolverDataOnlyLabel(msg.LabelName), "SystemMessage", "CapMessage");
	}

	#region Signals

	private void OnSelectNewTextSource()
	{
		var popup = SceneCreator<PopupMsbtSelectEntryOnlyLabel>.Create();

		popup.Archive = Project.Cache.ProjectLabelCache.ArchiveType.SYSTEM;
		popup.File = "CapMessage.msbt";

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

		var resolver = new NodeMessageResolverDataOnlyLabel(label);
		Content.Params["Text"] = resolver;

		SetLabelDisplayTextSource(resolver, arc, file);
		SetNodeModified();
	}

	#endregion

	#region Utilities

	protected void SetLabelDisplayTextSource(NodeMessageResolverDataOnlyLabel msg, string arcName, string file)
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
		SarcFile arc = holder.GetArchiveByFileName(arcName);
		var msbt = arc.GetFileMSBT(file + ".msbt", new MsbtElementFactoryProjectSmo());

		var txt = msbt.GetEntry(msg.LabelName);
		if (txt == null)
		{
			LabelTextSource.Modulate = Colors.Crimson;
			LabelTextSource.Text = Tr("EVENT_FLOW_NODE_MESSAGE_TALK_SOURCE_PLACEHOLDER");
			return;
		}

		TextMessagePreview.Text = txt.GetRawText(true);
	}

	protected override bool IsContainMessageResolver()
	{
		// This might be the ugliest code of all time
		// look i'm hungry okay?
		NodeMessageResolverDataOnlyLabel msgOL = null;

		if (!Content.TryGetParam("Text", out NodeMessageResolverData msg))
		{
			if (!Content.Params.ContainsKey("Text"))
				return false;
			
			msgOL = Content.Params["Text"] as NodeMessageResolverDataOnlyLabel;
		}

		if (msg != null)
			return msg.LabelName != null && msg.LabelName != string.Empty;
		else if (msgOL != null)
			return msgOL.LabelName != null && msgOL.LabelName != string.Empty;
		
		return false;
	}

	#endregion
}
