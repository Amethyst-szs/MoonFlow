using Godot;
using MoonFlow.Project.FTP;
using System;

namespace MoonFlow.Scene.Tools;

[SceneUid("uid://dlqa3a1xwd8sm"), Icon("res://asset/nindot/lms/icon/DeviceFont_Album.png")]
public partial class AlbumFtp : AppScene
{
    [Export, ExportGroup("Internal References")]
    private Control Container;

    public async override void _Ready()
    {
        byte[] file = await ProjectFtpClient.Client.DownloadBytes("/Nintendo/Album/2025/03/12/2025031210535800-8AEDFF741E2D23FBED39474178692DAF.jpg", 0);
        
        var image = new Image();
        image.LoadJpgFromBuffer(file);

        var tex = ImageTexture.CreateFromImage(image);
        var panel = new TextureRect
        {
            Texture = tex
        };

        Container.AddChild(panel);
    }
}
