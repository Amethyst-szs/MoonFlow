using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace MoonFlow;

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

    // ====================================================== //
    // ============== Task Reflection Utilities ============= //
    // ====================================================== //

    private static List<MethodInfo> GetAllTasks<TaskAttribute>()
    {
        List<MethodInfo> tasks = [];

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!assembly.GetName().Name.Equals("MoonFlow"))
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