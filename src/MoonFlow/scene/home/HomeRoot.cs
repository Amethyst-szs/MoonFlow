using Godot;
using System;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;

using MoonFlow.Scene.Dev;

namespace MoonFlow.Scene.Home;

[Icon("res://asset/app/icon/home.png"), ScenePath("res://scene/home/home.tscn")]
public partial class HomeRoot : AppScene
{
    protected override void AppInit()
    {
        if (!OS.IsDebugBuild())
            return;
    
        var app = SceneCreator<DevDebug>.Create();
        Scene.NodeApps.AddChild(app);
    }

    public override bool TryCloseFromTreeQuit(out SignalAwaiter awaiter)
    {
        awaiter = null;
        return true;
    }
}
