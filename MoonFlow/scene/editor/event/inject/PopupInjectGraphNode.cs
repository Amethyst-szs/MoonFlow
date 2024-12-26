using Godot;
using System;
using System.Linq;

using Nindot.Al.EventFlow.Smo;

using MoonFlow.Project;

namespace MoonFlow.Scene.EditorEvent;

[ScenePath("res://scene/editor/event/inject/popup_inject_graph_node.tscn")]
public partial class PopupInjectGraphNode : Popup
{
	public EventFlowApp Context { get; private set; } = null;

	[Export, ExportGroup("Internal References")]
	private VBoxContainer ContainerRoot;
	[Export]
	private VBoxContainer ContainerFav;

	public const string DefaultNodeName = "PopupInjectGraphNodeInstance";
	private GDScript DropdownButton = GD.Load<GDScript>("res://addons/ui_node_ext/dropdown_checkbox.gd");

	[Signal]
    public delegate void PinRemovedCommonEventHandler(string name);

    #region Initilization

    public override void _EnterTree()
    {
		Hide();
    }
    public override void _Ready()
    {
		// Initilize all favorite options
		var config = ProjectManager.GetProject().Config;
		foreach (var fav in config.Data.EventFlowGraphPins)
		{
			var button = new NodeInjectButton();
			button.SetupButton(this, fav);
			ContainerFav.AddChild(button);
		}

		// Create dropdowns for each category
		foreach (var cat in Enum.GetValues<MetaCategoryTable.Categories>())
			InitDropdownCategory(cat);
    }

	private void InitDropdownCategory(MetaCategoryTable.Categories cat)
	{
		// Get content list
		var content = MetaCategoryTable.Table.Where(c => c.Value == cat).ToList();
		if (content.Count == 0)
			return;
		
		// Create dropdown container
		VBoxContainer box = ContainerRoot;

		var color = MetaDefaultColorLookupTable.Lookup(cat);

		if (content.Count > 1)
		{
			var margin = new MarginContainer();
			box = new VBoxContainer()
			{
				Name = Enum.GetName(cat),
			};

			var dropdown = DropdownButton.New().As<Button>();
			dropdown.Text = Tr(Enum.GetName(cat), "GRAPH_NODE_CATEGORY_TABLE");
			dropdown.SelfModulate = color;

			dropdown.Set("dropdown", margin);

			ContainerRoot.AddChild(dropdown);
			ContainerRoot.AddChild(margin);
			margin.AddChild(box);
		}
		
		// Add items to local container
		foreach (var item in content)
		{
			var name = item.Key;

			var button = new NodeInjectButton();
			button.SetupButton(this, name);
			box.AddChild(button);
		}
	}

    public void SetupWithContext(EventFlowApp context)
	{
		Context = context;

		Position = (Vector2I)Context.GetGlobalMousePosition();
		Popup();
	}

	#endregion

	#region Signals

	public void OnInjectButtonPressed(string name)
	{
		if (!IsInstanceValid(Context))
			return;
		
		var node = ProjectSmoEventFlowFactory.CreateNode(Context.Graph, name);
		Context.Graph.AddNode(node);
		var nodeEdit = Context.InjectNewNode(node);

		nodeEdit.SetPosition(Context.GraphCanvas.InjectNodePosition);

		Hide();
	}

	public void OnInjectButtonPinned(string name)
	{
		// Update pin list
		var config = ProjectManager.GetProject().Config;
		if (config.Data.EventFlowGraphPins.Contains(name))
			return;
		
		config.Data.EventFlowGraphPins.Add(name);
		config.WriteFile();

		// Create new pin button in favorite container
		var button = new NodeInjectButton();
		button.SetupButton(this, name);
		ContainerFav.AddChild(button);
	}
	public void OnInjectButtonRemovePinned(string name)
	{
		// Update pin list
		var config = ProjectManager.GetProject().Config;
		if (!config.Data.EventFlowGraphPins.Contains(name))
			return;
		
		config.Data.EventFlowGraphPins.Remove(name);
		config.WriteFile();

		EmitSignal(SignalName.PinRemovedCommon, name);

		// Remove button from favorite container
		if (!ContainerFav.HasNode(name))
			return;
		
		ContainerFav.FindChild(name, false, false).QueueFree();
	}

	#endregion
}
