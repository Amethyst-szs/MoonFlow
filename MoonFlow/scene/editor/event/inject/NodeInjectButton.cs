using Godot;
using MoonFlow.Project;
using System;

namespace MoonFlow.Scene.EditorEvent;

public partial class NodeInjectButton : Button
{
	#region Properties

	// ~~~~~~~~~~~~~~~ Pinning ~~~~~~~~~~~~~~~ //

	private bool _isPin = false;
	private bool IsPin
	{
		get { return _isPin; }
		set
		{
			_isPin = value;
			
			if (value)
			{
				Icon = PinIcon;
				EmitSignal(SignalName.PinAdded, Name);
			}
			else
			{
				Icon = null;
				EmitSignal(SignalName.PinRemoved, Name);
			}
		}
	}

	private static readonly Texture2D PinIcon = GD.Load<Texture2D>("res://asset/material/graph/pin.svg");

	// ~~~~~~~~~~~~~~~ Signals ~~~~~~~~~~~~~~~ //

	[Signal]
    public delegate void PinAddedEventHandler(string name);
	[Signal]
    public delegate void PinRemovedEventHandler(string name);
	[Signal]
    public delegate void InjectSelectedEventHandler(string name);

	#endregion

    public override void _EnterTree()
    {
		ActionMode = ActionModeEnum.Press;
        ButtonMask = MouseButtonMask.Left | MouseButtonMask.Right;
		IconAlignment = HorizontalAlignment.Right;
    }
    public override void _Ready()
	{
		Pressed += OnPressed;
	}

	public void SetupButton(PopupInjectGraphNode context, string name)
	{
		// Setup self
		Name = name;
		Text = Tr(name, "EVENT_GRAPH_NODE_TYPE");

		var category = MetaCategoryTable.Lookup(name);
		var color = MetaDefaultColorLookupTable.Lookup(category);

		TooltipText = Tr(Enum.GetName(category), "GRAPH_NODE_CATEGORY_TABLE") + '\n' + name;
		SelfModulate = color.Lightened(0.15F);

		var config = ProjectManager.GetProject().Config;
		if (config.Data.EventFlowGraphPins.Contains(name))
		{
			_isPin = true;
			Icon = PinIcon;
		}

		// Connect self to context
		Connect(SignalName.InjectSelected, Callable.From(new Action<string>(context.OnInjectButtonPressed)));
		Connect(SignalName.PinAdded, Callable.From(new Action<string>(context.OnInjectButtonPinned)));
		Connect(SignalName.PinRemoved, Callable.From(new Action<string>(context.OnInjectButtonRemovePinned)));

		context.Connect(PopupInjectGraphNode.SignalName.PinRemovedCommon, Callable.From(
			new Action<string>(OnAnyPinRemoved)
		));
	}

	private void OnPressed()
	{
		var mask = Input.GetMouseButtonMask();
		if (mask == MouseButtonMask.Left)
		{
			EmitSignal(SignalName.InjectSelected, Name);
			return;
		}

		if (mask == MouseButtonMask.Right)
		{
			IsPin = !IsPin;
			return;
		}
	}

	private void OnAnyPinRemoved(string name)
	{
		if (name != Name)
			return;
		
		IsPin = false;
	}
}
