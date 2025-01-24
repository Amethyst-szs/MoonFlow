using System.Threading.Tasks;

namespace Godot.Extension;

public static partial class Extension
{
    public static async Task WaitProcessFrame(this GodotObject obj)
    {
        await obj.ToSignal(Engine.GetMainLoop(), "process_frame");
    }

    public static async Task WaitProcessFrame()
    {
        var obj = new GodotObject();
        await obj.ToSignal(Engine.GetMainLoop(), "process_frame");
        obj.Free();
    }
}