using Godot;
using MoonFlow.Project;
using System;

namespace MoonFlow.Scene;

[SceneUid("uid://brvdyoargiqjl")]
public partial class RecentEntryPanel : PanelContainer
{
    private string Path = null;
    
    [Export, ExportGroup("Internal References")]
    private Label LabelName;
    [Export]
    private Label LabelPath;

    [Export]
    private Label LabelAdditionalInfo;

    [Signal]
    public delegate void PanelPressedEventHandler(string path);

    public void SetupPanel(string path)
    {
        Path = path;

        // Load config data for project
        if (!ProjectManager.IsProjectConfigExist(ref path, out string projPath, false))
        {
            SetupPanelWithoutProjectConfig(path);
            return;
        }

        var config = new ProjectConfig(projPath);

        // Setup labels
        LabelName.Text = config.GetDisplayName();
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
        LabelName.Text = "PLACEHOLDER ERROR";
        LabelPath.Text = path.TrimSuffix("romfs/");
        LabelAdditionalInfo.Hide();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouse)
            return;
        
        if (mouse.ButtonIndex != MouseButton.Left || !mouse.Pressed)
            return;
        
        EmitSignalPanelPressed(Path);
    }
}
