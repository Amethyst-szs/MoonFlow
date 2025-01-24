using System;
using System.Threading.Tasks;

using FluentFTP;

namespace MoonFlow.Project;

public interface IProjectFtpStatusIndicator
{
    #region Status

    public void SetStatusActive();
    public void SetStatusConnecting();
    public void SetStatusConnected();
    public void SetStatusDisconnected();
    public void SetStatusDisabled();

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