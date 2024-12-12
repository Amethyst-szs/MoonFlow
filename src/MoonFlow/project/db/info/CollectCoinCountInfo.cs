using YamlDotNet.Serialization;

namespace MoonFlow.Project.Database;

public class CollectCoinCountInfo
{
    public int CollectCoinNum;
    public string WorldName;

    [YamlIgnore]
    public const string BymlPath = "CollectCoinNum.byml";
}