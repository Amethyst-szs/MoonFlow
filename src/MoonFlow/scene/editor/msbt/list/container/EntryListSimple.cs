using Godot;
using System;
using System.Linq;

using Nindot.LMS.Msbt;

namespace MoonFlow.Scene.EditorMsbt;

public partial class EntryListSimple(MsbtEditor parent) : EntryListBase(parent)
{
    public override void CreateContent(SarcMsbtFile file)
    {
        // Sort list of labels in alphabetical order
        var labelList = file.GetEntryLabels().ToArray();
        Array.Sort(labelList, string.Compare);

        foreach (var key in labelList)
            CreateEntryListButton(key, key, this, false);
    }

    public override Button CreateEntryListButton(string key, bool isSort = false)
    {
        return CreateEntryListButton(key, key, this, isSort);
    }

    public override Button CreateEntryListButton(string key, string label, Control container, bool isSort = false)
    {
        // Instantiate button
        var button = new Button
        {
            Name = key,
            Text = label,
            ToggleMode = true,
            TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,

            ExpandIcon = true,
            IconAlignment = HorizontalAlignment.Right
        };

        button.Connect(Button.SignalName.Pressed, Callable.From(() => OnEntrySelected(key)));
        button.Connect(Button.SignalName.ButtonDown, Callable.From(() => OnEntrySelected(key)));
        button.Connect(Button.SignalName.MouseEntered, Callable.From(() => OnEntryHovered(key)));

        // Add child to container
        container.AddChild(button);

        if (!isSort)
            return button;

        int moveIndex = 0;
        for (int i = 0; i < GetChildCount(); i++)
        {
            int result = string.Compare(key, GetChild(i).Name);
            if (result > 0)
            {
                moveIndex += 1;
                continue;
            }

            break;
        }

        MoveChild(button, moveIndex);
        return button;
    }
}
