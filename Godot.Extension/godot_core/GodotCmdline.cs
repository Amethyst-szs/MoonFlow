using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Godot.Extension;

public static partial class Cmdline
{
    private static Dictionary<string, string> Args = null;

    [StartupTask]
    private static void InitArgList()
    {
        if (Args != null)
            return;
        
        Args = [];
        
        foreach (var arg in OS.GetCmdlineUserArgs().Select(arg => arg.Split('=')))
        {
            if (arg.Length == 1) Args.Add(arg[0], null);
            else Args.Add(arg[0], arg[1]);
        }
    }

    public static ReadOnlyDictionary<string, string> GetArgs()
    {
        if (Args == null)
            InitArgList();
        
        return new ReadOnlyDictionary<string, string>(Args);
    }
}