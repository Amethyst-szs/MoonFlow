using Godot;
using System;

using Nindot.LMS.Msbt;

namespace MoonFlow.Scene.EditorMsbt;

public partial class EntryListSimple : EntryListBase
{
    public override void CreateContent(SarcMsbtFile file, out string[] labels)
    {
        // Sort list of labels in alphabetical order
        labels = [.. file.GetEntryLabels()];
        Array.Sort(labels, string.Compare);

        foreach (var key in labels)
            CreateEntryListButton(key, key, this, false);
    }

    public override void CreateEntryListButton(string key, bool isSort = false)
    {
        CreateEntryListButton(key, key, this, isSort);
    }

    public override void CreateEntryListButton(string key, string label, Control container, bool isSort = false)
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
            return;

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
    }
}
