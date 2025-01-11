using Godot;
using System;
using System.Linq;

using Nindot.LMS.Msbt;

namespace MoonFlow.Scene.EditorMsbt;

public abstract partial class EntryListBase : VBoxContainer
{
    public MsbtEditor Editor = null;

    public Button EntryListSelection { get; private set; } = null;
    public Label EntryCount { get; private set; } = null;

    protected static readonly Texture2D ModifiedTexture = GD.Load<Texture2D>("res://asset/material/file/modify.svg");

    [Signal]
    public delegate void EntrySelectedEventHandler(string label);

    public override void _Ready()
    {
        if (!IsInstanceValid(Editor))
            throw new NullReferenceException(nameof(Editor));

        // Get references
        EntryCount = GetNode<PanelContainer>("../../Controls").Get("label_entry_count").As<Label>();

        // Connect to signals from parent
        Editor.Connect(MsbtEditor.SignalName.ContentModified,
            Callable.From(new Action<string>(OnContentModified)));

        // Connect parent to our signals
        Connect(EntryListBase.SignalName.EntrySelected,
            Callable.From(new Action<string>(Editor.OnEntryListSelection)));
    }

    public abstract void CreateContent(SarcMsbtFile file, out string[] labels);
    public abstract void CreateEntryListButton(string key, bool isSort = false);
    public abstract void CreateEntryListButton(string key, string label, Control container, bool isSort = false);

    // ====================================================== //
    // ==================== Signal Events =================== //
    // ====================================================== //

    #region Signals (Buttons)

    public void OnEntryHovered(string label)
    {
        if (this.IsAnyChildFocused() && Input.IsMouseButtonPressed(MouseButton.Left))
            OnEntrySelected(label);
    }

    public void OnEntrySelected(string label, bool isGrabFocus = true)
    {
        // Close old selection
        if (IsInstanceValid(EntryListSelection))
            EntryListSelection.ButtonPressed = false;

        if (IsInstanceValid(Editor.EntryContentSelection))
            Editor.EntryContentSelection.Hide();

        // Ensure string is not empty
        if (label == string.Empty)
            return;

        // Open entry
        var button = FindChild(label, true, false) as Button;
        EntryListSelection = button;

        if (button == null)
            return;

        button.ButtonPressed = true;
        if (isGrabFocus)
            button.GrabFocus();

        // If this button is part of a DropdownButton container, search upward 
        // in search of a margin container to enable visibility on
        var scan = button.GetParent();
        while (scan.GetType() != typeof(EntryListHolder) && scan != null)
        {
            if (scan.GetType() == typeof(MarginContainer))
            {
                (scan as MarginContainer).Show();

                if (scan.HasMeta("dropdown"))
                    scan.GetMeta("dropdown").As<Button>().ButtonPressed = true;

                break;
            }

            scan = scan.GetParent();
        }

        // Emit signal for main editor to handle editor content
        EmitSignal(SignalName.EntrySelected, label);
    }

    #endregion

    #region Search

    private string EntrySearchString = "";

    public void UpdateEntryCount()
    {
        OnSearchEntryListUpdated(EntrySearchString);
    }

    public void UpdateSearch(string searchStr)
    {
        EntrySearchString = searchStr;
        OnSearchEntryListUpdated(EntrySearchString);
    }

    protected void OnSearchEntryListUpdated(string match)
    {
        // Update string
        EntrySearchString = match;

        // Update entry count in other components
        int entryCount = Editor.File.GetEntryCount();

        // If search is cleared, show all
        if (match == string.Empty)
        {
            ShowAllEntries(this);
            UpdateDropdownMenuContainers();

            UpdateEntryCountLabel(entryCount, entryCount);
            return;
        }

        // Set item visiblity
        int matchCount = 0;
        UpdateEntryVisiblity(this, ref matchCount);
        UpdateDropdownMenuContainers();

        UpdateEntryCountLabel(entryCount, matchCount);

        // If the current selection is no longer visible, update selection to first visible option
        if (EntryListSelection == null || !EntryListSelection.Visible)
            UpdateSelectionToFirstVisibleItem(this);
        else
            SetSelection(EntryListSelection.Name, false);
    }

    private static void ShowAllEntries(Node node)
    {
        if (node.GetType() == typeof(Button))
        {
            (node as Button).Show();
            return;
        }

        if (node.GetChildCount() == 0)
            return;

        foreach (var child in node.GetChildren())
            ShowAllEntries(child);
    }

    private void UpdateEntryVisiblity(Node node, ref int matchCount)
    {
        var name = node.Name.ToString();

        if (node.GetType() == typeof(Button) && !name.EndsWith("_Dropdown"))
        {
            var isMatch = name.Contains(EntrySearchString, StringComparison.OrdinalIgnoreCase);
            (node as Button).Visible = isMatch;
            matchCount += isMatch ? 1 : 0;
        }

        if (node.GetChildCount() == 0)
            return;

        foreach (var child in node.GetChildren())
            UpdateEntryVisiblity(child, ref matchCount);
    }

    private void UpdateSelectionToFirstVisibleItem(Node node)
    {
        if (node.GetType() == typeof(Button) && (node as Button).Visible)
        {
            SetSelection(node.Name, false);
            return;
        }

        if (node.GetChildCount() == 0)
            return;

        foreach (var child in node.GetChildren())
            UpdateSelectionToFirstVisibleItem(child);
    }

    private void UpdateDropdownMenuContainers()
    {
        foreach (var child in GetChildren())
        {
            if (child.GetType() != typeof(MarginContainer))
                continue;

            var margin = child as MarginContainer;
            margin.Hide();

            if (!margin.HasMeta("dropdown"))
                continue;

            var dropdown = margin.GetMeta("dropdown").As<Button>();
            dropdown.ButtonPressed = false;
            dropdown.Visible = IsDropdownMenuVisibleChildren(margin);
        }
    }

    protected void UpdateEntryCountLabel(int total) { UpdateEntryCountLabel(total, total); }

    protected void UpdateEntryCountLabel(int total, int matching)
    {
        if (total == matching)
        {
            EntryCount.Text = total.ToString() + " Entries";
            return;
        }

        EntryCount.Text = string.Format("{0}/{1} Entries", matching, total);
    }

    private static bool IsDropdownMenuEmpty(MarginContainer root)
    {
        return root.GetChildCount() == 1 && root.GetChild(0).GetChildCount() == 0;
    }

    private static bool IsDropdownMenuVisibleChildren(MarginContainer root)
    {
        if (IsDropdownMenuEmpty(root))
            return false;

        var list = root.GetChild(0).GetChildren();
        return list.Any((s) => (s as Control).Visible);
    }

    #endregion

    #region Utility

    public void OnContentModified(string label)
    {
        if (label == string.Empty)
            return;

        var entryButton = FindChild(label, true, false) as Button;

        if (entryButton.Icon != ModifiedTexture)
            entryButton.Icon = ModifiedTexture;
    }

    public static void ClearAllModifiedIcons(Node node)
    {
        if (node.GetType() == typeof(Button) && !node.Name.ToString().EndsWith("_Dropdown"))
            (node as Button).Icon = null;

        foreach (var child in node.GetChildren())
            ClearAllModifiedIcons(child);
    }

    public async void SetSelection(string str, bool isGrabFocus = true)
    {
        await ToSignal(Engine.GetMainLoop(), "process_frame");
        OnEntrySelected(str, isGrabFocus);
    }

    #endregion
}
