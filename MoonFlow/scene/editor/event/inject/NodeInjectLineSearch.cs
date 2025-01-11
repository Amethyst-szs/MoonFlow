using Godot;
using System.Linq;

using MoonFlow.Project;

namespace MoonFlow.Scene.EditorEvent;

public partial class NodeInjectLineSearch : LineEdit
{
	private const string PlaceholderTextBase = "EVENT_FLOW_GRAPH_INJECT_SEARCH_BOX_PLACEHOLDER_TEXT";

	public override void _Ready()
	{
		// Respond to visibility change events
		VisibilityChanged += OnVisiblityChanged;
	}

	private void OnVisiblityChanged()
	{
		// Set random placeholder text
		var idx = (int)(GD.Randi() % MetaCategoryTable.Table.Count);
		var suffix = MetaCategoryTable.Table.Keys.ElementAt(idx);

		PlaceholderText = string.Format("{0} \"{1}\"", Tr(PlaceholderTextBase), Tr(suffix, "EVENT_GRAPH_NODE_TYPE"));

		// Attempt to grab focus
		CallDeferred(MethodName.GrabFocus);
	}
}
