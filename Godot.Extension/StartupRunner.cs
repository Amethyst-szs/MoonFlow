using System;
using System.Collections.Generic;
using System.Reflection;

namespace Godot.Extension;

// Flag a method as something to run during TaskRunner init (before scene)
[AttributeUsage(AttributeTargets.Method)]
public class StartupTask : Attribute;

public partial class StartupRunner : Node
{
    public override void _Ready()
    {
        List<MethodInfo> startupTasks = GetAllTasks<StartupTask>();
        foreach (var task in startupTasks) { task.Invoke(null, []); }
    }

    private static List<MethodInfo> GetAllTasks<TaskAttribute>()
    {
        List<MethodInfo> tasks = [];

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = assembly.GetName().Name;
            if (!name.StartsWith("MoonFlow"))
                continue;

            foreach (Type type in assembly.GetTypes())
            {
                var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var method in methods)
                {
                    if (method.GetCustomAttribute(typeof(TaskAttribute)) == null)
                        continue;

                    tasks.Add(method);
                    break;
                }
            }
        }

        return tasks;
    }
}