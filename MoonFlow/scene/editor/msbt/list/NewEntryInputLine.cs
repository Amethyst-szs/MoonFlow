using System;
using System.Linq;
using Godot;

namespace MoonFlow.Scene.EditorMsbt;

public partial class NewEntryInputLine : LineEdit
{
    private EntryListHolder Holder = null;
    private TextureRect InvalidWarning = null;

    public override void _Ready()
    {
        Holder = GetNode<EntryListHolder>("../../../../");
        InvalidWarning = GetNode<TextureRect>("../Texture_Warning");
    }

    private void OnAddEntryNameChanged(string name)
    {
        var isValid = IsAddEntryNameValid(name);
        InvalidWarning.Visible = !isValid;
    }

    private void OnAddEntryNameSubmitted(string name)
    {
        if (!IsAddEntryNameValid(name))
            return;

        Holder.EmitSignal(EntryListHolder.SignalName.CreateEntry, name);
    }

    private void OnEntrySubmitButtonPressed()
    {
        OnAddEntryNameSubmitted(Text);
    }

    private bool IsAddEntryNameValid(string name)
    {
        if (name == string.Empty)
            return false;

        if (name.Contains(' '))
            return false;

        var editor = GetEditor();
        if (editor.File.GetEntryLabels().Contains(name))
            return false;

        byte[] bytes = name.ToCharArray().Select(c => (byte)c).ToArray();
        string decodedString = System.Text.Encoding.UTF8.GetString(bytes);

        return name.Equals(decodedString);
    }

    private MsbtEditor GetEditor()
    {
        return Holder.Editor;
    }
}
