using System.Collections.Generic;
using Godot;
using MoonFlow.Project;

using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.Scene.EditorMsbt;

public partial class MsbtPageEditor : TextEdit
{
    private const string TextureDirectory = "res://asset/nindot/lms/icon/";
    private Dictionary<string, Texture2D> TagTextureTable = [];
    private Texture2D UnknownTagTexture = null;

    private ProjectIconResolver ProjectTagIconResolver = ProjectManager.GetMSBTArchives().ProjectIconResolver;

    public override void _Draw()
    {
        if (Page == null)
            return;
        
        foreach (var item in Page)
        {
            if (item.IsText())
                continue;

            // Calculate the rect of this tag's glyph
            int charIdx = Page.CalcCharPosForElement(item);
            CalcLineColumnAtCharIdx(charIdx, out int line, out int col);
            Rect2I glyphRect = GetRectAtLineColumn(line, col);

            // Conform glyph to 1:1 aspect ratio
            int sizeDif = glyphRect.Size.Y - glyphRect.Size.X;
            glyphRect = new(glyphRect.Position.X + (sizeDif / 4), glyphRect.Position.Y + (sizeDif / 4),
                glyphRect.Size.X, glyphRect.Size.X);

            if (item.IsTag())
            {
                var tag = (MsbtTagElement)item;
                var tex = GetTagTexture(tag);

                var color = tag.GetModulateColor(Project);
                if (color != System.Drawing.Color.White)
                {
                    var colorGodot = Color.Color8(color.R, color.G, color.B, color.A);
                    DrawTextureRect(tex, glyphRect, false, colorGodot);
                }
                else
                {
                    DrawTextureRect(tex, glyphRect, false, null);
                }
            }

            if (item.IsTagClose())
                DrawRect(glyphRect, new Color(1, 1, 1));
        }
    }

    private void CalcLineColumnAtCharIdx(int charIdx, out int line, out int col)
    {
        line = 0;
        col = 0;

        while (GetLineCount() > line)
        {
            string lineStr = GetLine(line);

            if (lineStr.Length < charIdx)
            {
                line++;
                charIdx -= lineStr.Length + 1;
                continue;
            }

            col = charIdx;
            return;
        }
    }

    public Texture2D GetTagTexture(MsbtTagElement tag)
    {
        // Get texture name from either the tag or icon resolver
        string textureName;

        if (tag is not MsbtTagElementProjectTag)
            textureName = tag.GetTextureName((int)ProjectManager.GetRomfsVersion());
        else
        {
            textureName = tag.GetTextureName(0);
            List<string> texList = ProjectTagIconResolver.ResolveTextureNames(textureName);

            if (texList.Count == 0)
                return null;

            textureName = texList[0];
        }

        // If this texture doesn't exist in the tag table, add it
        TryRegisterTagTexture(textureName);

        // Get the texture and return texture if successful
        TagTextureTable.TryGetValue(textureName, out Texture2D tex);
        if (tex != null) return tex;

        // If getting the texture failed, return the default UnknownTagTexture
        UnknownTagTexture ??= (Texture2D)GD.Load("res://iconS.png");
        return UnknownTagTexture;
    }

    private void TryRegisterTagTexture(string name)
    {
        if (TagTextureTable.ContainsKey(name))
            return;

        string filePath = GetTextureFilePath(name);

        var tex = (Texture2D)GD.Load(filePath);
        TagTextureTable[name] = tex;
    }

    private static string GetTextureFilePath(string name)
    {
        return TextureDirectory + name + ".png";
    }
}
