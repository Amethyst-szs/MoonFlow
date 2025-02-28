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
        Vector4 s = new();

        // THIS CODE IS 100% ABSOLUTELY COMPLETELY WRONG!
        // Needs fixing cause it doesn't work at all lmao

        Matrix4x4 MVP = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
        MVP[0,0] = ViewMatrix[0,0];
        MVP[0,1] = ViewMatrix[0,1];
        MVP[0,2] = ViewMatrix[0,2];
        MVP[0,3] = ViewMatrix[0,3];
        MVP[1,0] = ViewMatrix[1,0];
        MVP[1,1] = ViewMatrix[1,1];
        MVP[1,2] = ViewMatrix[1,2];
        MVP[1,3] = ViewMatrix[1,3];
        MVP[2,0] = ViewMatrix[2,0];
        MVP[2,1] = ViewMatrix[2,1];
        MVP[2,2] = ViewMatrix[2,2];
        MVP[2,3] = ViewMatrix[2,3];
        
        // Matrix4x4.Invert(ViewProjMatrix, out Matrix4x4 MVP);
        // Matrix4x4 MVP = ;

        s[0] = (world[0] * MVP[0, 0]) + (world[1] * MVP[1, 0]) + (world[2] * MVP[2, 0]) + MVP[3, 0];
        s[1] = (world[0] * MVP[0, 1]) + (world[1] * MVP[1, 1]) + (world[2] * MVP[2, 1]) + MVP[3, 1];
        s[2] = (world[0] * MVP[0, 2]) + (world[1] * MVP[1, 2]) + (world[2] * MVP[2, 2]) + MVP[3, 2];
        s[3] = (world[0] * MVP[0, 3]) + (world[1] * MVP[1, 3]) + (world[2] * MVP[2, 3]) + MVP[3, 3];

        Vector3 screen = new();
        screen[0] = s[0] / s[3] * screenSize.X / 2.0f + screenSize.X / 2.0f;
        screen[1] = s[1] / s[3] * screenSize.Y / 2.0f + screenSize.Y / 2.0f;
        screen[2] = s[2] / s[3];

        return screen;
    }
}