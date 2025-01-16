using System;
using System.Threading.Tasks;

namespace MoonFlow.Project;

public interface IProjectLoadingScene
{
    public void LoadingStart(Task task, string remoteName, string remoteHash, long remoteTimeU);
    public void LoadingUpdateProgress(string key);
    public void LoadingUpdateProgress(string key, string suffix);
    public void LoadingComplete();
    public void LoadingException(Exception e);
}