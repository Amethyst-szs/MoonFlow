using System.Numerics;

using Godot.Extension.Resources;

namespace MoonFlow.Project;

public class Map2d(string name, BfresResource bfres)
{
    public readonly Godot.Image Texture = bfres.GetImage(name);

    public Matrix4x4 ProjMatrix { get; internal set; } = Matrix4x4.Identity;
    public Syroot.Maths.Matrix4x3 ViewMatrix { get; internal set; } = Syroot.Maths.Matrix4x3.Zero;
    public Matrix4x4 ViewProjMatrix { get; internal set; } = Matrix4x4.Identity;

    public Vector3 CalcMapTrans(Vector3 world, Vector2 screenSize)
    {
        Vector4 s = Vector4.Transform(new Vector4(world, 1), Matrix4x4.Transpose(ViewProjMatrix));

        Vector3 screen = new Vector3(s.X / s.W, s.Y / s.W, s.Z / s.W);
        screen.Y = -screen.Y;
        screen.X += 1.0f;
        screen.Y += 1.0f;
        screen.X *= screenSize.X / 2.0f;
        screen.Y *= screenSize.Y / 2.0f;

        return screen;
    }
}