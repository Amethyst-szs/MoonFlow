using Godot;
using MoonFlow.Project;
using System;
using System.Linq;

namespace MoonFlow.Scene;

[SceneUid("uid://brvdyoargiqjl")]
public partial class RecentEntryPanel : PanelContainer
{
    private string Path = null;
    private bool IsValid = true;
    
    [Export, ExportGroup("Internal References")]
    private Label LabelName;
    [Export]
    private Label LabelPath;

    [Export]
    private Label LabelAdditionalInfo;
    [Export]
    private Label LabelWarnMissing;

    [Signal]
    public delegate void PanelPressedEventHandler(string path, bool isDeleteFromHistory);

    public void SetupPanel(string path)
    {
        Path = path;

        // Load config data for project
        if (!ProjectManager.IsProjectConfigExist(ref path, out string projPath, false))
        {
            SetupPanelWithoutProjectConfig(path);
            return;
        }

        LabelWarnMissing.Hide();

        var config = new ProjectConfig(projPath);

        // Setup labels
        if (config.IsDisplayNameDefault())
            LabelName.Hide();

        // LabelName.Text = config.GetDisplayName();
        LabelName.Text = path.TrimSuffix("romfs/").Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries).Last();
        LabelPath.Text = path.TrimSuffix("romfs/");

        string additionalInfo = "";

        // Add string for romfs version target
        additionalInfo += 'v' + ((int)config.GetRomfsVersion()).ToString(@"#\.#\.#") + " - ";
        // Add string for default language
        additionalInfo += Tr(config.GetDefaultLanguage(), LangPicker.DisplayNameContext);

        LabelAdditionalInfo.Text = additionalInfo;
    }

    private void SetupPanelWithoutProjectConfig(string path)
    {
        IsValid = false;

        LabelName.Hide();
        LabelPath.Text = path.TrimSuffix("romfs/");

        LabelAdditionalInfo.Hide();
        LabelWarnMissing.Show();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouse)
            return;
        
        if (mouse.ButtonIndex != MouseButton.Left || !mouse.Pressed)
            return;
        
        EmitSignalPanelPressed(Path, !IsValid);
    }

    private void OnPanelTrashPressed()
    {
        EmitSignalPanelPressed(Path, true);
    }
}
