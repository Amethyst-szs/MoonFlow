using Godot;
using System;

namespace MoonFlow.Scene.EditorMsbt;

[GlobalClass]
public partial class EntryLabelButtonState : Resource
{
    [Export]
    public Texture2D Icon;
    [Export]
    public string TextBankKey = "None";
    [Export]
    public Color Modulate = Colors.White;

    public const string TextContext = "MSBT_ENTRY_LABEL_BUTTON_TOOLTIP";

    public void SetButtonToState(EntryLabelButton button)
    {
        button.TooltipText = Tr(TextBankKey, TextContext);
        button.SelfModulate = Modulate;

        if (button.Icon != Icon)
            button.Icon = Icon;
    }
}
