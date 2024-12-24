using Godot;
using System;

namespace MoonFlow.Scene.EditorWorld;

public partial class OptionSourceType : OptionButton
{
	public enum Options
	{
		Path,
		MapUnit,
		Tutorial,
	}

	[Signal]
    public delegate void OptionSelectedEventHandler();

	private const string TrContext = "EVENT_FLOW_NODE_MESSAGE_BALLOON_OPTIONS";

	public override void _Ready()
	{
		RegisterOption(Options.Path);
		RegisterOption(Options.MapUnit);
		RegisterOption(Options.Tutorial);

		Connect(SignalName.ItemSelected, Callable.From(new Action<int>(OnItemSelected)));
	}

	public void SetupSelection(string nodeName)
	{
		Selected = (int)EvaluateName(nodeName);
	}

	private static Options EvaluateName(string name) => name switch
	{
		{} when name.Contains("MapUnit") => Options.MapUnit,
		{} when name.Contains("Tutorial") => Options.Tutorial,
		_ => Options.Path,
	};

	#region Utilities

	private void OnItemSelected(int idx)
	{
		EmitSignal(SignalName.OptionSelected);
	}

	private void RegisterOption(Options opt)
	{
		string name = Enum.GetName(opt);

		var idx = ItemCount;
		AddItem(Tr(name, TrContext), (int)opt);

		var tooltipName = name + "Tooltip";
		SetItemTooltip(idx, Tr(tooltipName, TrContext));
	}

	#endregion
}
