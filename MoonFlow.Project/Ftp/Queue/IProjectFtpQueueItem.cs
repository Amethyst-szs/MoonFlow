using System;
using System.Threading.Tasks;
using FluentFTP;

namespace MoonFlow.Project.FTP;

internal interface IProjectFtpQueueItem
{
    public Task<bool> Process();

    public string GetPath();
    public void SetPath(string path);
    
    public EventHandler<FtpProgress> GetCallback();

    public bool IsUnique(IProjectFtpQueueItem comparison);
}