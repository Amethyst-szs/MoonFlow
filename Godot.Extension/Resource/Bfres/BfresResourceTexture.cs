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
    public Image GetImage(string texture) => GetImage(texture, out _);
    public Image GetImage(string texture, out SwitchTexture raw)
    {
        var tex = GetTextureRaw(texture);
        if (tex is not SwitchTexture swtex)
        {
            raw = null;
            return null;
        }

        raw = swtex;

        int w = (int)swtex.Width;
        int h = (int)swtex.Height;
        var deswis = swtex.GetDeswizzledData(0, 0);

        // This conversion switch case should be expanded to improve compatibility
        // with more texture formats!
        switch (swtex.Format)
        {
            // Handle BC1-BC3 using built-in godot formats
            case SurfaceFormat.BC1_SRGB:
            case SurfaceFormat.BC1_UNORM: return Image.CreateFromData(w, h, false, Image.Format.Dxt1, deswis);
            case SurfaceFormat.BC2_SRGB:
            case SurfaceFormat.BC2_UNORM: return Image.CreateFromData(w, h, false, Image.Format.Dxt3, deswis);
            case SurfaceFormat.BC3_SRGB:
            case SurfaceFormat.BC3_UNORM: return Image.CreateFromData(w, h, false, Image.Format.Dxt5, deswis);

            // Raw formats
            case SurfaceFormat.B8_G8_R8_A8_SRGB:
            case SurfaceFormat.B8_G8_R8_A8_UNORM: return Image.CreateFromData(w, h, false, Image.Format.Rgba8, deswis);

            case SurfaceFormat.A4_B4_G4_R4_UNORM: return Image.CreateFromData(w, h, false, Image.Format.Rgba4444, deswis);

            // Handle ETC formats
            case SurfaceFormat.ETC1_SRGB:
            case SurfaceFormat.ETC1_UNORM: return Image.CreateFromData(w, h, false, Image.Format.Etc, deswis);
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

        return ImageTexture.CreateFromImage(img);;
    }
}