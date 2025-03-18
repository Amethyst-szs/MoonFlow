using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentFTP;
using FluentFTP.Helpers;

namespace MoonFlow.Scene.Tools;

public class AlbumListingHolder
{
    internal class AlbumDirectoryMonth
    {
    }

    internal class AlbumDirectoryYear(string year)
    {
        public List<AlbumDirectoryMonth> Months { get; private set; } = [];

        public string Year { get; private set; } = year;
        public string Path => PathBase + Year + "/";

        public async void UpdateListing(AlbumFtpClient client)
        {
            // TODO: Implement further tree structure
            var list = await client.GetListing(Path, FtpListOption.Auto);
            return;
        }
    }

    internal const string PathBase = "/Nintendo/Album/";
    internal List<AlbumDirectoryYear> Content = [];

    public async void Init(AlbumFtpClient client)
    {
        var years = await client.GetListing(PathBase);

        foreach (var name in years.Reverse().Select(n => n.Name))
        {
            var year = new AlbumDirectoryYear(name);
            year.UpdateListing(client);

            Content.Add(year);
        }
    }    
}