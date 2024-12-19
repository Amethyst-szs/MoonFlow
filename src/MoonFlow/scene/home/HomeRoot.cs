using Godot;

using MoonFlow.Project;

namespace MoonFlow.Scene.Home;

[Icon("res://asset/app/icon/home.png"), ScenePath("res://scene/home/home.tscn")]
public partial class HomeRoot : AppScene
{
    public override bool TryCloseFromTreeQuit(out SignalAwaiter awaiter)
    {
        awaiter = null;
        return true;
    }
}
