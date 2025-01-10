using Godot;
using Godot.Collections;
using System.Linq;

using Nindot.LMS.Msbt;
using System;

namespace MoonFlow.Scene.EditorMsbt;

public partial class EntryListStageMessage : EntryListBase
{
    // Really ugly but works well, instantiates a GDScript file as a RefCounted and gets table as dict
    private readonly static RefCounted CategoryTableScript =
        GD.Load<GDScript>("res://scene/editor/msbt/list/container/stage_message_category_table.gd")
        .New().As<RefCounted>();
    private readonly static Dictionary<string, string> CategoryTable =
        CategoryTableScript.Get("table").AsGodotDictionary<string, string>();

    // Load DropdownButton node class
    private readonly static GDScript DropdownButton =
        GD.Load<GDScript>("res://scene/common/button/dropdown_checkbox.gd");

    public override void CreateContent(SarcMsbtFile file, out string[] labels)
    {
        // If the node already contains content, remove and free all
        if (GetChildCount() > 0)
        {
            foreach (var child in GetChildren())
            {
                RemoveChild(child);
                child.QueueFree();
            }
        }

        // Sort list of labels in alphabetical order
        labels = [.. file.GetEntryLabels()];
        System.Array.Sort(labels, (a, b) =>
        {
            int aPrefixIdx = a.IndexOf('_');
            int bPrefixIdx = b.IndexOf('_');

            if (aPrefixIdx != -1 && bPrefixIdx != -1 && a[..aPrefixIdx] == b[..bPrefixIdx])
            {
                int dif = GetObjId(a) - GetObjId(b);
                if (dif != 0)
                    return dif;
            }

            return string.Compare(a, b);
        });

        CreateStageMessageContainers();

        foreach (var key in labels)
            CreateEntryListButton(key);
    }

    public override void CreateEntryListButton(string key, bool isSort = false)
    {
        var container = GetContainer(key, out string label);
        CreateEntryListButton(key, label, container, isSort);
    }

    public override void CreateEntryListButton(string key, string label, Control container, bool isSort = false)
    {
        // If sorting is required, reload entire EntryList
        if (isSort)
        {
            CreateContent(Editor.File, out _);
            SetSelection(key, false);
            return;
        }

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
        return;
    }

    public void CreateStageMessageContainers()
    {
        // Instantiate dropdown boxes for each category in table
        foreach (var cat in CategoryTable)
        {
            // Create margin and vbox
            var margin = new MarginContainer() { Name = cat.Key + "_Margin" };
            var box = new VBoxContainer { Name = cat.Key };

            // Get category texture if it exists
            var tex = CategoryTableScript.Get(cat.Key + "_tex").As<Texture2D>();

            // Create dropdown button
            var dropdown = DropdownButton.New().As<Button>();
            dropdown.Name = cat.Key + "_Dropdown";
            dropdown.Text = cat.Value;
            dropdown.Alignment = HorizontalAlignment.Center;

            dropdown.Icon = tex;
            dropdown.IconAlignment = HorizontalAlignment.Right;
            dropdown.ExpandIcon = true;

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
        label = key;

        int idx = System.Array.FindIndex([.. CategoryTable.Keys], key.StartsWith);
        if (idx != -1)
            return FindChild(CategoryTable.Keys.ElementAt(idx), true, false) as Control;

        return this;
    }

    private static int GetObjId(string input)
    {
        if (!char.IsDigit(input.Last()))
            return -1;

        // Black magic code from Stack Overflow :D
        return int.Parse(new string(input.Reverse().TakeWhile(char.IsDigit).Reverse().ToArray()));
    }
}
