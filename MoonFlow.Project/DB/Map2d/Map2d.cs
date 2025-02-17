using System.Numerics;

using Godot;
using Godot.Extension.Resources;

using Syroot.Maths;

using YamlDotNet.Serialization;

namespace MoonFlow.Project;

public class Map2d(string name, BfresResource bfres)
{
    public readonly Image Texture = bfres.GetImage(name);

    public Matrix4x4 ProjMatrix { get; private set; } = Matrix4x4.Identity;
    public Matrix3x4 ViewMatrix { get; private set; } = Matrix3x4.Zero;
    public Matrix4x4 ViewProjMatrix { get; private set; } = Matrix4x4.Identity;
}