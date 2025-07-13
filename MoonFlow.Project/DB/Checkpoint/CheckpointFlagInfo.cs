using System.Numerics;
using YamlDotNet.Serialization;

namespace MoonFlow.Project.Database;

public class CheckpointFlagInfo
{
    public CheckpointFlagInfo() {}
    public CheckpointFlagInfo(string id, Vector3 position)
    {
        FlagIdStr = id;
        Trans = position;
    }

    public string FlagIdStr;
    public Vector3 Trans;
}