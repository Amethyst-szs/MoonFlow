using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;

using Nindot;

using FluentFTP;

namespace MoonFlow.Project.FTP;

public static class ProjectFtpClient
{
    private static AsyncFtpClient Client = null;
    private static string ProjectPath = null;

    public static readonly ProjectFtpTargettingConfig Target = new();

    #region Connection

    public static async Task Connect(ProjectFtpConnectionRequest request)
    {
        if (Client != null && Client.IsConnected)
            return;

        Client ??= new AsyncFtpClient();
        Client.Host = request.Hostname;
        Client.Port = request.Port;
        Client.Credentials.UserName = request.Username;
        Client.Credentials.Password = request.Password;

        await Client.Connect();

        if (await Client.IsStillConnected())
        {
            Console.WriteLine("FTP: Connected");
            return;
        }

        Console.WriteLine("FTP: Connection failed");
        return;
    }

    public static void Disconnect()
    {
        if (Client == null)
            return;

        Client.Dispose();
        Client = null;

        Console.WriteLine("FTP: Disconnected");
    }

    public static void SetupProjectPath(string path) { ProjectPath = path; }

    public static bool IsConnected()
    {
        if (Client == null) return false;
        return Client.IsAuthenticated;
    }
    public static async Task<bool> IsConnectedStill()
    {
        if (Client == null) return false;
        return await Client.IsStillConnected();
    }

    #endregion

    #region Upload

    public static void UploadSarc(SarcFile file, EventHandler<FtpProgress> callback = null)
    {
        // Convert sarc file into bytes
        var data = file.GetBytes();

        // Resolve target directory
        if (ProjectPath == null || ProjectPath == string.Empty)
            throw new NullReferenceException("No project path defined!");
        
        if (!file.FilePath.StartsWith(ProjectPath))
            throw new Exception("SarcFile is not contained within project!");

        string remotePath = Target.GetTarget() + file.FilePath.TrimPrefix(ProjectPath);

        // Create callback holder if requested
        Progress<FtpProgress> callbackHolder = null;
        if (callback != null)
        {
            callbackHolder = new();
            callbackHolder.ProgressChanged += callback;
        }

        // Queue upload
        Client.UploadBytes(data, remotePath, FtpRemoteExists.Overwrite, true, callbackHolder);
    }

    #endregion
}