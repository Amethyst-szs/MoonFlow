using System;
using System.Threading.Tasks;

using FluentFTP;

namespace MoonFlow.Project;

public interface IProjectFtpStatusIndicator
{
    #region Status

    public void SetStatusActive(bool isForce = false);
    public void SetStatusConnecting(bool isForce = false);
    public void SetStatusConnected(bool isForce = false);
    public void SetStatusDisconnected(bool isForce = false);
    public void SetStatusDisabled(bool isForce = false);

    #endregion

    #region Progress Updates

    public void OnProgressUpdate(FtpProgress data);

    #endregion

    #region Signals

    public void AttachEventConnected(Action func);
    public void AttachEventDisconnected(Action func);
    public void DetachEventConnected(Action func);
    public void DetachEventDisconnected(Action func);

    public void EmitEventConnected();
    public void EmitEventDisconnected();

    #endregion
}