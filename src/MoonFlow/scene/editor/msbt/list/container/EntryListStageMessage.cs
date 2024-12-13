using Godot;
using Godot.Collections;
using System.Linq;

using Nindot.LMS.Msbt;
using System;

namespace MoonFlow.Scene.EditorMsbt;

public partial class EntryListStageMessage : EntryListBase
{
    // Really ugly but works well, instantiates a GDScript file as a RefCounted and gets table as dict
    private readonly static Dictionary<string, string> CategoryTable = GD.Load<GDScript>(
        "res://scene/editor/msbt/list/container/stage_message_category_table.gd"
    ).New().As<RefCounted>().Get("table").AsGodotDictionary<string, string>();

    private readonly static GDScript DropdownButton = GD.Load<GDScript>(
        "res://addons/ui_node_ext/dropdown_checkbox.gd"
    );

    public override void CreateContent(SarcMsbtFile file)
    {
        // Sort list of labels in alphabetical order
        var labelList = file.GetEntryLabels().ToArray();
        System.Array.Sort(labelList, string.Compare);

        CreateStageMessageContainers();

        foreach (var key in labelList)
            CreateEntryListButton(key);
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
            IconAlignment = HorizontalAlignment.Right,
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
        // Instantiate dropdown boxes for each category in table
        foreach (var cat in CategoryTable)
        {
            // Create margin and vbox
            var margin = new MarginContainer();
            var box = new VBoxContainer { Name = cat.Key };

            // Create dropdown button
            var dropdown = DropdownButton.New().As<Button>();
            dropdown.Name = cat.Key + "_Dropdown";
            dropdown.Text = cat.Value;
            dropdown.Alignment = HorizontalAlignment.Center;

            dropdown.Set("dropdown", margin);

            box.Connect(SignalName.ChildEnteredTree, Callable.From(
                new Action<Node>((_) => dropdown.CallDeferred("show"))
            ));

            AddChild(dropdown);

            // Insert list into tree
            margin.AddChild(box);
            AddChild(margin);

            dropdown.CallDeferred("hide");
            margin.CallDeferred("hide");
        }
    }

    private Control GetContainer(string key, out string label)
    {
        int idx = System.Array.FindIndex([.. CategoryTable.Keys], key.StartsWith);
        if (idx != -1)
        {
            label = key[(key.Find("_") + 1)..];
            return FindChild(CategoryTable.Keys.ElementAt(idx), true, false) as Control;
        }

        label = key;
        return this;
    }
}
