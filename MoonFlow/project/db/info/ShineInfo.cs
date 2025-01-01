using System.Numerics;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;

namespace MoonFlow.Project.Database;

public class ShineInfo
{
    public string StageName;
    public string ScenarioName;

    public string ObjId;
    public int UniqueId;

    public int HintIdx;
    public int MainScenarioNo;
    public int ProgressBitFlag; // Each bit corresponds to if the shine is collectable in that scenario

    public bool IsAchievement;
    public bool IsGrand;
    public bool IsMoonRock;

    public Vector3 Trans = Vector3.Zero;

    public MsbtEntry LookupDisplayName()
    {
        var msbtHolder = ProjectManager.GetMSBTArchives()?.StageMessage;
        if (msbtHolder == null)
            return null;

        if (!msbtHolder.Content.ContainsKey(StageName + ".msbt"))
            return null;
        
        var txtFile = msbtHolder.GetFileMSBT(StageName + ".msbt", new MsbtElementFactory());
        var label = "ScenarioName_" + ObjId;

        if (!txtFile.IsContainKey(label))
            return null;
        
        return txtFile.GetEntry(label);
    }
}