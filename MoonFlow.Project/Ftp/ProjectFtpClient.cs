using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Extension;

using Nindot;

using FluentFTP;

namespace MoonFlow.Project.FTP;

public static class ProjectFtpClient
{
    private static AsyncFtpClient Client = null;
    public readonly static ProjectFtpCredentialStore CredentialStore = new();
    private static string ProjectPath = null;

    public static readonly ProjectFtpTargettingConfig Target = new();

    private static IProjectFtpStatusIndicator StatusIndicator = null;

    #region Connection

    public static void Init(IProjectFtpStatusIndicator statusIndicator)
    {
        StatusIndicator = statusIndicator;
        _ = TryConnect();
    }

    public static async Task<bool> TryConnect()
    {
        if (StatusIndicator == null)
            throw new NullReferenceException("Don't try to connect before Init function is called!");
        
        if (Client != null && Client.IsConnected)
            Disconnect();

        if (CredentialStore.Host == string.Empty)
        {
            StatusIndicator.SetStatusDisabled();
            return false;
        }

        Client ??= new AsyncFtpClient();
        Client.Host = CredentialStore.Host;
        Client.Port = CredentialStore.Port;
        Client.Credentials.UserName = CredentialStore.User;
        Client.Credentials.Password = CredentialStore.Pass;

        StatusIndicator.SetStatusConnecting();

        try { await Client.Connect(); }
        catch
        {
            StatusIndicator.SetStatusDisconnected();
            Client.Dispose();
            Client = null;
            return false;
        }

        if (await Client.IsStillConnected())
        {
            StatusIndicator.SetStatusConnected();
            GD.Print("FTP: Connected");
            return true;
        }

        StatusIndicator.SetStatusDisconnected();
        GD.Print("FTP: Connection failed");
        return false;
    }

    public static void Disconnect()
    {
        if (Client == null)
            return;

        Client.Dispose();
        Client = null;

        StatusIndicator?.SetStatusDisconnected();

        GD.Print("FTP: Disconnected");
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