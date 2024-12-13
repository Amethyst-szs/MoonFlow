using Godot;
using System;
using System.Linq;

using Nindot.LMS.Msbt;

namespace MoonFlow.Scene.EditorMsbt;

public partial class EntryListStageMessage : EntryListBase
{
    public override void CreateContent(SarcMsbtFile file)
    {
        // Sort list of labels in alphabetical order
        var labelList = file.GetEntryLabels().ToArray();
        Array.Sort(labelList, string.Compare);

        // Create additional containers if this is a stage message
        bool isStageMessage = Editor.IsStageMessage();
        if (isStageMessage)
            CreateStageMessageContainers();

        foreach (var key in labelList)
        {
            var container = GetContainer(key, out string label);
            CreateEntryListButton(key, label, container);
        }
    }

    public override Button CreateEntryListButton(string key, bool isSort = false)
    {
        var container = GetContainer(key, out string label);
        return CreateEntryListButton(key, label, container, isSort);
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

    public void CreateStageMessageContainers()
    {
        return;
    }

    private Control GetContainer(string key, out string label)
    {
        label = key;
        return this;
    }
}
