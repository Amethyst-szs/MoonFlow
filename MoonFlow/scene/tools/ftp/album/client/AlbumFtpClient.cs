using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using MoonFlow.Project.FTP;

namespace MoonFlow.Scene.Tools;

public class AlbumFtpClient() : AsyncFtpClient()
{
    private readonly AlbumListingHolder Listing = new();

    public async void InitAlbumClient()
    {
        // Copy configuration from ProjectFtpClient credentials
        Host = ProjectFtpClient.CredentialStore.Host;
        Port = ProjectFtpClient.CredentialStore.Port;
        Credentials.UserName = ProjectFtpClient.CredentialStore.User;
        Credentials.Password = ProjectFtpClient.CredentialStore.Pass;

        // Attempt connection
        try
        {
            await Connect();
        }
        catch
        {
        }

        Listing.Init(this);
    }
}