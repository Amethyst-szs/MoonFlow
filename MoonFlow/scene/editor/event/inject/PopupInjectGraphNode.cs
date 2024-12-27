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
			box = new VBoxContainer()
			{
				Name = Enum.GetName(cat),
			};

			var dropdown = DropdownButton.New().As<Button>();
			dropdown.Text = Tr(Enum.GetName(cat), "GRAPH_NODE_CATEGORY_TABLE");
			dropdown.SelfModulate = color;

			dropdown.Set("dropdown", box);

			ContainerRoot.AddChild(dropdown);
			ContainerRoot.AddChild(box);
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

	public void SetupWithContextCentered(EventFlowApp context)
	{
		Context = context;
		PopupCentered();
	}

	#endregion

	#region Signals

	private void OnSearchUpdated(string txt)
	{
		if (txt == string.Empty)
		{
			ShowAllButtons(ContainerRoot);
			ShowAllButtons(ContainerFav);
			return;
		}

		// This fixes a weird bug, lazy af fix I know
		SetButtonVisiblity(ContainerRoot, txt);
		SetButtonVisiblity(ContainerRoot, txt);

		foreach (var child in ContainerFav.GetChildren())
			if (child is Button button)
				button.Hide();
	}

	public void OnInjectButtonPressed(string name)
	{
		if (!IsInstanceValid(Context))
			return;

		// If adding an entry point, handle differently
		if (name == "EntryPoint")
		{
			var next = GetNextValidEntryPointName();
			Context.Graph.EntryPoints.Add(next, null);

			var entryEdit = Context.InjectNewEntryPoint(next);

			entryEdit.SetPosition(Context.GraphCanvas.InjectNodePosition);
			Hide();

			return;
		}

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

	#region Utilities

	private void ShowAllButtons(Node root)
	{
		if (root != ContainerRoot && root != ContainerFav)
		{
			if (root is Button button)
			{
				button.Visible = true;
				button.ButtonPressed = false;
			}

			if (root is VBoxContainer box)
				box.Hide();
		}

		foreach (var child in root.GetChildren())
			ShowAllButtons(child);
	}

	private void SetButtonVisiblity(Node root, string term)
	{
		if (root != ContainerRoot)
		{
			if (root is Button button)
			{
				if (button.GetScript().As<Script>() == DropdownButton)
				{
					var menu = button.Get("dropdown").As<VBoxContainer>();
					bool state = menu.GetChildren().Any(c => c.Get("visible").AsBool());

					menu.Visible = state;
					button.Visible = false;
				}
				else
				{
					var str = button.Name.ToString();
					var str2 = button.Text;

					button.Visible = str.Contains(term, StringComparison.OrdinalIgnoreCase);
					button.Visible |= str2.Contains(term, StringComparison.OrdinalIgnoreCase);
				}
			}
		}

		foreach (var child in root.GetChildren())
			SetButtonVisiblity(child, term);
	}

	private string GetNextValidEntryPointName()
	{
		var points = Context.Graph.EntryPoints.Keys;
		int suffixID = 1;

		while (true)
		{
			var str = "EntryPoint_" + suffixID.ToString();
			if (!points.Contains(str))
				return str;

			suffixID++;
		}
	}

	#endregion
}
