using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Godot;

using Nindot;

using FluentFTP;

namespace MoonFlow.Project.FTP;

public static partial class ProjectFtpClient
{
    internal static AsyncFtpClient Client { get; private set; } = null;
    public readonly static ProjectFtpCredentialStore CredentialStore = new();

    private readonly static ProjectDirectoryLocalWatcher Project = new();
    private static string ProjectPath { get => Project?.Path; }

    public static IProjectFtpStatusIndicator StatusIndicator { get; private set; } = null;

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
            StatusIndicator.EmitEventDisconnected();
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
        catch { return OnConnectionFailure(); }

        if (!await Client.IsStillConnected())
            return OnConnectionFailure();

        await UpdateWorkingDirectory();

        StatusIndicator.SetStatusConnected();
        StatusIndicator.EmitEventConnected();

        GD.Print("FTP: Connected");
        return true;
    }
    private static bool OnConnectionFailure()
    {
        Client?.Dispose();
        Client = null;

        StatusIndicator.SetStatusDisconnected();
        StatusIndicator.EmitEventDisconnected();

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
        StatusIndicator?.EmitEventDisconnected();

        GD.Print("FTP: Disconnected");
    }

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

    #region Server Access

    public static void UploadFile(SarcFile sarc, EventHandler<FtpProgress> callback = null)
    {
        UploadFile(sarc.FilePath, callback);
    }
    public static void UploadFile(string path, EventHandler<FtpProgress> callback = null)
    {
        var item = new ProjectFtpQueueUpdateFile(path, false, callback);
        if (IsQueueItemAlreadyExist(item))
            return;

        RemoteQueue.Enqueue(item);
        TryStartupQueue();
    }

    public static void RenameFile(string oldPath, string newPath)
    {
        var item = new ProjectFtpQueueRenameFile(oldPath, newPath);
        if (IsQueueItemAlreadyExist(item))
            return;

        RemoteQueue.Enqueue(item);
        TryStartupQueue();
    }
    public static void RenameDirectory(string oldPath, string newPath)
    {
        var item = new ProjectFtpQueueRenameDirectory(oldPath, newPath);
        if (IsQueueItemAlreadyExist(item))
            return;

        RemoteQueue.Enqueue(item);
        TryStartupQueue();
    }

    internal static void PushToQueue<T>(string path) where T : IProjectFtpQueueItem, new()
    {
        var item = new T();
        item.SetPath(path);

        if (IsQueueItemAlreadyExist(item))
            return;

        RemoteQueue.Enqueue(item);
        TryStartupQueue();
    }

    #endregion

    #region Utility

    public static void UpdateLocalProjectDirectory(string path) => Project.AttachToProject(path);
    public static async Task UpdateWorkingDirectory()
    {
        if (Client == null || !Client.IsAuthenticated)
            return;

        var dir = CredentialStore.TargetDirectory;

        if (!await Client.DirectoryExists(dir))
            await Client.CreateDirectory(dir);

        await Client.SetWorkingDirectory(dir);
    }

    internal static string CalcServerPathFromProjectPath(string path)
    {
        if (!path.StartsWith(ProjectPath))
            throw new Exception("Cannot upload file that is not contained within project!");

        return CredentialStore.TargetDirectory + path.TrimPrefix(ProjectPath);
    }

    internal static Progress<FtpProgress> TryCreateProgressCallbackHolder(EventHandler<FtpProgress> callback)
    {
        Progress<FtpProgress> callbackHolder = new();

        callbackHolder.ProgressChanged += (obj, prog) =>
        {
            StatusIndicator?.OnProgressUpdate(prog);
            callback?.Invoke(obj, prog);
        };

        return callbackHolder;
    }

    #endregion
}