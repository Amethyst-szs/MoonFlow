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

    public abstract void CreateContent(SarcMsbtFile file);
    public abstract Button CreateEntryListButton(string key, bool isSort = false);
    public abstract Button CreateEntryListButton(string key, string label, Control container, bool isSort = false);

    // ====================================================== //
    // ==================== Signal Events =================== //
    // ====================================================== //

    #region Signals (Buttons)

    public void OnEntryHovered(string label)
    {
        if (Input.IsMouseButtonPressed(MouseButton.Left))
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
        // Update entry count in other components
        int entryCount = Editor.File.GetEntryCount();

        // Update visiblity of selectors
        if (match == string.Empty)
        {
            foreach (var child in GetChildren())
                ((Control)child).Show();

            UpdateEntryCountLabel(entryCount, entryCount);
            return;
        }

        int matching = 0;
        Node firstMatch = null;
        bool isSearchForNewSelection = true;

        foreach (var child in GetChildren())
        {
            if (child.GetType() != typeof(Button)) continue;

            var isMatch = child.Name.ToString().Contains(match, StringComparison.OrdinalIgnoreCase);
            ((Button)child).Visible = isMatch;
            matching += isMatch ? 1 : 0;

            if (firstMatch == null && isMatch)
                firstMatch = child;

            if (!isMatch && (EntryListSelection == child || isSearchForNewSelection))
            {
                if (GetChildCount() == 0)
                {
                    OnEntrySelected("", false);
                    continue;
                }

                if (isSearchForNewSelection && firstMatch != null)
                {
                    isSearchForNewSelection = false;
                    OnEntrySelected(firstMatch.Name, false);
                    continue;
                }
            }
        }

        UpdateEntryCountLabel(entryCount, matching);
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

    #endregion

    #region Utility

    public void OnContentModified(string label)
    {
        var entryButton = FindChild(label, true, false) as Button;

        if (entryButton.Icon != ModifiedTexture)
            entryButton.Icon = ModifiedTexture;
    }

    public async void SetSelection(string str)
    {
        await ToSignal(Engine.GetMainLoop(), "process_frame");
        OnEntrySelected(str);
    }

    #endregion
}
