using System;
using System.Collections.Generic;

namespace MoonFlow.Project.Database;

public class WorldInfo()
{
    public string Name = null;
    public string WorldName = null;
    public string Display = null;

    public List<StageInfo> StageList = [];
    public List<ShineInfo> ShineList = [];

    public enum WorldID
    {
        CAP = 0,
        WATERFALL = 1,
        SAND = 2,
        FOREST = 3,
        LAKE = 4,
        CLOUD = 5,
        CLASH = 6,
        CITY = 7,
        SEA = 8,
        SNOW = 9,
        LAVA = 10,
        ATTACK = 11,
        SKY = 12,
        MOON = 13,
        PEACH = 14,
        SPECIAL1 = 15,
        SPECIAL2 = 16
    };

    public const string WorldListPath = "SystemData/WorldList.szs";
    public const string ShineInfoPath = "SystemData/ShineInfo.szs";
    public const string DatabaseBymlPath = "WorldListFromDb.byml";
}