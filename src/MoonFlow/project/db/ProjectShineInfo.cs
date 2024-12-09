using System.Collections.Generic;
using System.Numerics;
using Godot;

namespace MoonFlow.Project.Database;

public class ShineInfo
{
    public string StageName = null;
    public string ObjId = null;
    public string UniqueId = null;

    public string IsAchievement = null;
    public string IsGrand = null;
    public string IsMoonRock = null;

    public System.Numerics.Vector3 Trans = System.Numerics.Vector3.Zero;
}