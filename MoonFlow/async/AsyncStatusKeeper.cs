using System;
using System.Collections.Generic;

namespace MoonFlow.Async;

public static class AsyncStatusKeeper
{
    private static readonly List<string> ActiveTaskIds = [];

    public static void RegisterTaskId(string id) => ActiveTaskIds.Add(id);
    public static void RemoveTaskId(string id) => ActiveTaskIds.Remove(id);
    public static bool IsTaskIdRunning(string id) => ActiveTaskIds.Contains(id);
}