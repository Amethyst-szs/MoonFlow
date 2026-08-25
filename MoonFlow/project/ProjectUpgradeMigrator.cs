using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace MoonFlow.Project;

[AttributeUsage(AttributeTargets.Method)]
public class ProjectUpgradeMigrationMethod(long timeThreshold) : Attribute
{
    public long TimeThreshold = timeThreshold;
}

public static class ProjectUpgradeMigrator
{
    [ProjectUpgradeMigrationMethod(1787624238)]
    public static void FixShineInfoFromBeforeObjectNameAndIsShopProperty(ProjectState state)
    {
        // A list of the shine UIDs that are used for the shop moons and a hash code
        // that will only match if the shine is unmodified from the base game
        Dictionary<int, uint> UidToHashTable = new() {
            { 230, 2220214199 },
            { 211, 319551988 },
            { 565, 1019169634 },
            { 138, 4010515396 },
            { 430, 3182783818 },
            { 398, 2989406618 },
            { 101, 2084294050 },
            { 460, 2593714861 },
            { 868, 4203588611 },
            { 294, 1857427069 },
            { 360, 992245372 },
            { 1157, 1472008250 },
            { 933, 2859046011 },
        };

        int modifiedCount = 0;

        var worldList = state.Database.WorldList;
        foreach (var world in worldList)
        {
            foreach (var shine in world.ShineList)
            {
                if (!UidToHashTable.TryGetValue(shine.UniqueId, out uint cmpHash))
                    continue;
                
                // Calculate this shine's hash with the given project customization
                // If it doesn't match, do not modify to prevent breaking user content
                uint hash = (shine.ObjId + shine.HintIdx + shine.StageName + shine.Trans).ToString().Hash();
                if (hash != cmpHash)
                    continue;
    
                shine.IsShop = true;
                modifiedCount += 1;

                GD.Print("Shine UID: \"" + shine.UniqueId.ToString() + "\" given IsShop property by ProjectUpgradeMigrator");
            }
        }

        if (modifiedCount > 0)
            state.Database.WriteShineInfoAllWorlds();

        GD.Print("ProjectUpgradeMigrator: Modified " + modifiedCount + " shines by setting IsShop property to true");
        return;
    }
}