using System;

using FluentFTP;

namespace MoonFlow.Project.FTP;

internal struct ProjectFtpQueueItem(string path, EventHandler<FtpProgress> callback = null)
{
    internal string Path = path;
    internal EventHandler<FtpProgress> Callback = callback;
}