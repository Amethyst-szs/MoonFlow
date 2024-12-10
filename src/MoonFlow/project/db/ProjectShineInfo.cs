using System;

namespace MoonFlow.Project.Database;

public class ShineInfo
{
    public string StageName;
    public string ObjectName;
    public string ScenarioName;

    public string ObjId;
    public int UniqueId;

    public int HintIdx;
    public int MainScenarioNo;
    public int ProgressBitFlag; // Each bit corresponds to if the shine is collectable in that scenario

    public bool IsAchievement;
    public bool IsGrand;
    public bool IsMoonRock;

    public System.Numerics.Vector3 Trans = System.Numerics.Vector3.Zero;
}