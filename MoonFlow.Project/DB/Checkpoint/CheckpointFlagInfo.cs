using System.Numerics;
using YamlDotNet.Serialization;

namespace MoonFlow.Project.Database;

public class CheckpointFlagInfo(string id, Vector3 position)
{
    public string FlagIdStr = id;
    public Vector3 Trans = position;
}