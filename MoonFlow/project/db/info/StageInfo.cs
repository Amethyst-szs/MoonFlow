using System;
using Godot;
using YamlDotNet.Serialization;

namespace MoonFlow.Project.Database;

#pragma warning disable IDE1006 // Naming Styles

public class StageInfo : IComparable
{
    public string name = null;

    [YamlIgnore]
    private string _category = null;
    public string category
    {
        get { return _category; }
        private set
        {
            _category = value;

            if (!Enum.TryParse<CatEnum>(category, out CatEnum e))
            {
                GD.PushWarning("Unknown StageInfo category - ", value);
                CategoryType = CatEnum.Unknown;
                return;
            }

            _catType = e;
        }
    }

    [YamlIgnore]
    private CatEnum _catType = CatEnum.Unknown;
    [YamlIgnore]
    public CatEnum CategoryType
    {
        get { return _catType; }
        set
        {
            _catType = value;

            if (value != CatEnum.Unknown)
                _category = Enum.GetName(value);
        }
    }
    public enum CatEnum : int
    {
        MainStage,
        MainRouteStage,
        PathwayStage,

        ExStage,
        MoonExStage,
        MoonFarSideExStage,

        SmallStage,
        BossRevenge,
        MiniGame,
        ShopStage,

        Zone,
        Demo,
        
        Unknown = 0xFFFF
    }

    public int CompareTo(object obj)
    {
        if (obj is not StageInfo)
            throw new Exception("Only compare to other StageInfo types!");
        
        var b = obj as StageInfo;

        var cmp = _catType - b._catType;
        if (cmp == 0)
            return name.CompareTo(b.name);
        
        return cmp;
    }
}

#pragma warning restore IDE1006 // Naming Styles