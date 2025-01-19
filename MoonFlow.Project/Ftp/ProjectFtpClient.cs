using System.Net;

using FluentFTP;

using Godot.Extension;

namespace MoonFlow.Project.FTP;

public static class ProjectFtpClient
{
    [StartupTask]
    public static void Connect()
    {
        var client = new FtpClient("192.168.0.6", "crafty", "boss", 5000);
        client.AutoConnect();

        var list = client.GetListing();
        foreach (var item in list)
            System.Console.WriteLine(item.Name);
    }
}