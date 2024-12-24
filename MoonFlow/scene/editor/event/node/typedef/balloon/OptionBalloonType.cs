using Godot;
using System;
using System.Linq;

namespace MoonFlow.Scene.EditorWorld;

public partial class OptionBalloonType : OptionButton
{
	public enum Options
	{
		MiniBalloon,
		TalkBalloon,
		IconBalloon,
	}

	[Signal]
    public delegate void OptionSelectedEventHandler();

	private const string TrContext = "EVENT_FLOW_NODE_MESSAGE_BALLOON_OPTIONS";

	public override void _Ready()
	{
		RegisterOption(Options.MiniBalloon);
		RegisterOption(Options.TalkBalloon);
		RegisterOption(Options.IconBalloon);

		Connect(SignalName.ItemSelected, Callable.From(new Action<int>(OnItemSelected)));
	}

	public void SetupSelection(string nodeName)
	{
		Selected = (int)EvaluateName(nodeName);
	}

	private static Options EvaluateName(string name) => name switch
	{
		{} when name.Contains("TalkBalloon") => Options.TalkBalloon,
		{} when name.Contains("IconBalloon") => Options.IconBalloon,
		_ => Options.MiniBalloon,
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
