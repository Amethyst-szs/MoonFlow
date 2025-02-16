using System;
using System.Linq;
using Godot;

using BfresLibrary;
using BfresLibrary.Switch;

using Syroot.NintenTools.NSW.Bntx.GFX;

namespace Godot.Extension.Resources;

public partial class BfresResource : ResFile
{
    public bool ContainsTexture(string texture) => Textures.Keys.Contains(texture);
    public string[] GetTextureList() => (string[])Textures.Keys;

    public TextureShared GetTextureRaw(string texture)
    {
        Textures.TryGetValue(texture, out TextureShared tex);
        return tex;
    }
    public Image GetImage(string texture)
    {
        var tex = GetTextureRaw(texture);
        if (tex is not SwitchTexture swtex)
            return null;
        
        int w = (int)swtex.Width;
        int h = (int)swtex.Height;
        
        switch(swtex.Format)
        {
            case SurfaceFormat.BC1_SRGB:
            case SurfaceFormat.BC1_UNORM:
                var deswis = swtex.GetDeswizzledData(0, 0);
                return Image.CreateFromData(w, h, false, Image.Format.Dxt1, deswis);
            default:
                GD.PushWarning("Cannot handle format " + swtex.Format.ToString());
                break;
        }

        return null;
    }
    public ImageTexture GetImageTexture(string texture)
    {
        var img = GetImage(texture);
        if (img == null) return null;

        return ImageTexture.CreateFromImage(img);
    }
}