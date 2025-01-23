using System;
using System.Threading.Tasks;

namespace MoonFlow.Project;

public interface IProjectFtpStatusIndicator
{
    public void SetStatusActive();
    public void SetStatusConnecting();

    public void SetStatusConnected();
    public void SetStatusDisconnected();
    
    public void SetStatusDisabled();
}