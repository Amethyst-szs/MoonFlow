using Godot;
using System;

using MoonFlow.Project;

namespace MoonFlow.Scene.EditorMsbt;

[SceneUid("uid://cb8bqvfemf6dj")]
public partial class ColorTagSceneSwatch : Button
{
    [Export]
    private Shader GradiationShader;

    public void Init(ProjectColorResolver.ColorGradiation info)
    {
        TooltipText = info.Name;

        var shaderMat = new ShaderMaterial { Shader = (Shader)GradiationShader.Duplicate() };
        Material = shaderMat;

        shaderMat.SetShaderParameter("first_color", info.Top);
        shaderMat.SetShaderParameter("second_color", info.Bottom);
    }
}
