using Godot;
using System;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.Scene.Home;

[Icon("res://asset/nindot/lms/icon/DeviceFont_ButtonHome.png"), ScenePath("res://scene/home/home.tscn")]
public partial class HomeRoot : AppScene
{
    public override bool TryCloseFromTreeQuit(out SignalAwaiter awaiter)
    {
        awaiter = null;
        return true;
    }
}
