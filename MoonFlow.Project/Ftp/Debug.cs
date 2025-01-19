using System.Threading.Tasks;
using Godot.Extension;

namespace MoonFlow.Project.FTP;

public static class DebugShiz
{
    [StartupTask]
    public static async Task DebugFunc()
    {
        // var request = new ProjectFtpConnectionRequest("192.168.0.6", 5000, "crafty", "boss");
        // await ProjectFtpClient.Connect(request);
        // ProjectFtpClient.Upload();
    }
}