using System.Linq;
using System.Text.RegularExpressions;
using Godot;

namespace MoonFlow.Scene.EditorMsbt;

public partial class NewEntryInputLine : LineEdit
{
    private EntryListHolder Holder = null;
    private TextureRect InvalidWarning = null;

    [GeneratedRegex(@"[^A-Za-z0-9@_-]")]
	private static partial Regex InvalidInputTest();

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

        var labels = GetEditor().File.GetEntryLabels().Select(s => s.ToNodeName());
        if (labels.Contains(name.ToNodeName()))
            return false;
        
        return !InvalidInputTest().IsMatch(name);
    }

    private MsbtEditor GetEditor()
    {
        return Holder.Editor;
    }
}
