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
    public override bool TryCloseFromTreeQuit(out SignalAwaiter awaiter)
    {
        awaiter = null;
        return true;
    }
}
