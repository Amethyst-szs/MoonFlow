using System;
using System.Linq;
using System.Numerics;

using YamlDotNet.Serialization;

using Nindot;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;

namespace MoonFlow.Project.Database;

public class ShineInfo
{
    public string StageName;
    public string ScenarioName;
    public string ObjectName;

    public string ObjId;
    public int UniqueId;
    public string OptionalId;

    public int HintIdx;
    public int MainScenarioNo;
    public int ProgressBitFlag; // Each bit corresponds to if the shine is collectable in that scenario

    public bool IsAchievement;
    public bool IsGrand;
    public bool IsMoonRock;

    public Vector3 Trans = Vector3.Zero;

    /// <summary>
    /// This property is not stored in the BYAML. Instead, it is a bool
    /// representing the game's hardcoded string comparison. True when ObjectName == "ショップ店員",
    /// False when any other name. Set this true/false to set ObjectName to
    /// "ショップ店員" / "シャイン" ("Shop staff member" / "Shine")
    /// </summary>
    [YamlIgnore]
    public bool IsShop
    {
        get { return ObjectName == "ショップ店員"; }
        set { if (IsShop != value) ObjectName = value ? "ショップ店員" : "シャイン"; }
    }

    public ShineInfo() { }

    public MsbtEntry LookupDisplayName(SarcFile stageMessage)
    {
        if (stageMessage == null || !stageMessage.Content.ContainsKey(StageName + ".msbt"))
            return null;

        var txtFile = stageMessage.GetFileMSBT(StageName + ".msbt", new MsbtElementFactory());
        var label = "ScenarioName_" + ObjId;

        if (!txtFile.IsContainKey(label))
            return null;

        return txtFile.GetEntry(label);
    }

    public void ReassignUID(ProjectDatabaseHolder database)
    {
        int uid = 0;
        while (true)
        {
            if (database.GetShineByUID(uid) != null)
            {
                uid++;
                continue;
            }

            UniqueId = uid;
            return;
        }
    }
    public bool IsUIDUnique(ProjectDatabaseHolder database)
    {
        var db = database
        ?? throw new NullReferenceException("Cannot get project database!");

        return db.GetShineCountWithUID(UniqueId) < 2;
    }

    public void ReassignHintId(WorldInfo world)
    {
        int hint = 0;
        while (true)
        {
            if (!IsHintIdUnique(world, hint))
            {
                hint++;
                continue;
            }

            HintIdx = hint;
            return;
        }
    }
    public bool IsHintIdUnique(WorldInfo world) { return IsHintIdUnique(world, HintIdx); }
    public bool IsHintIdUnique(WorldInfo world, int id)
    {
        return !world.ShineList.Any(s => s != this && s.HintIdx == id);
    }
}