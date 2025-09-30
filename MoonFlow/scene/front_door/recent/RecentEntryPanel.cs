using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;

using MoonFlow.Project;

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
        Hide();
        Task.Run(() => SetupPanelAsyncTask(path));
    }
    private void SetupPanelAsyncTask(string path)
    {
        // Load config data for project
        if (!ProjectManager.IsProjectConfigExist(ref path, out string projPath, false))
        {
            SetupPanelWithoutProjectConfig(path);
            return;
        }

        LabelWarnMissing.CallDeferred(MethodName.Hide);

        // Load information from this project's config file
        // This can take a bit especially for network directories, hence why this is an async task
        var config = new ProjectConfig(projPath);
        string nickname = config.LocalConfig.Data.ProjectNickname;

        if (nickname != null)
        {
            LabelName.SetDeferred(Label.PropertyName.Text, nickname);
        }
        else
        {
            string lName = path.TrimSuffix("romfs/").Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries).Last();
            LabelName.SetDeferred(Label.PropertyName.Text, lName);
        }

        LabelPath.SetDeferred(Label.PropertyName.Text, path.TrimSuffix("romfs/"));

        // Setup additional info string
        string additionalInfo = "";
        // Add string for romfs version target
        additionalInfo += 'v' + ((int)config.GetRomfsVersion()).ToString(@"#\.#\.#") + " - ";
        // Add string for default language
        additionalInfo += TranslationServer.Translate(config.GetDefaultLanguage(), LangPicker.DisplayNameContext);

        LabelAdditionalInfo.SetDeferred(Label.PropertyName.Text, additionalInfo);

        CallDeferred(MethodName.Show);
    }
    private void SetupPanelWithoutProjectConfig(string path)
    {
        IsValid = false;

        LabelName.CallDeferred(MethodName.Hide);
        LabelPath.SetDeferred(Label.PropertyName.Text, path.TrimSuffix("romfs/"));

        LabelAdditionalInfo.CallDeferred(MethodName.Hide);
        LabelWarnMissing.CallDeferred(MethodName.Show);

        CallDeferred(MethodName.Show);
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
