using System.Threading.Tasks;
using Godot;

using MoonFlow.Scene;

namespace MoonFlow.Project;

public class ProjectState
{
    public string Path { get; private set; } = null;
    private bool IsInitComplete = false;

    public ProjectState(string path, ConfigFile config, ProjectLoading loadScreen)
    {
        Path = path;
        InitProject(config, loadScreen);
    }

    public async void InitProject(ConfigFile config, ProjectLoading loadScreen)
    {
        // Get file as test
        var file = await RomfsAccessor.TryGetFileAsync("ObjectData/BandMan.szs");
        loadScreen.LoadingUpdateProgress(loadScreen.Msg_Temp);

        // Complete Initilization
        IsInitComplete = true;
        loadScreen.LoadingComplete();
    }

    public bool IsReady() { return IsInitComplete; }
}