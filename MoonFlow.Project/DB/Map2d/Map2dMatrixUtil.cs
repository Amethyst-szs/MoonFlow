using System.Numerics;

using Godot;
using System.Collections.Generic;
using System.Linq;
using System;
using Syroot.Maths;

namespace MoonFlow.Project;

public partial class Map2d
{
    private static Matrix4x4 ImportMatrix4x4Data(IEnumerable<float> data)
    {
        if (data.Count() < (4 * 4))
                throw new Exception("Not enough data to build matrix!");

        var matrix = new Matrix4x4();
        int offset = 0;

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                matrix[row, col] = data.ElementAt(offset);
                offset++;
            }
        }

        return matrix;
    }
    private static Matrix4x4 ImportMatrix3x4Data(IEnumerable<float> data)
    {
        if (data.Count() < (3 * 4))
            throw new Exception("Not enough data to build matrix!");

        var matrix = new Matrix4x4();
        int offset = 0;

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                matrix[row, col] = data.ElementAt(offset);
                offset++;
            }
        }

        return matrix;
    }

    public void GetInternalMatrices(out Matrix4x4 proj, out Matrix4x4 view)
    {
        proj = ProjMatrix;
        view = ViewMatrix;
    }
    public void SetInternalMatrices(Matrix4x4 proj, Matrix4x4 view)
    {
        ProjMatrix = proj;
        ViewMatrix = view;
        RecalculateViewProjMatrix();
    }

    public void CenterViewMatrixToOrigin()
    {
        ViewMatrix.M14 = 0.0f;
        ViewMatrix.M24 = 0.0f;
    }
    public void DragViewMatrix(Godot.Vector2 vec)
    {
        ViewMatrix.M14 += vec.X;
        ViewMatrix.M24 -= vec.Y;
    }
    public void RotateViewMatrix(float degree)
    {
        float rad = (float)(Math.PI / 180) * degree;
        var quat = System.Numerics.Quaternion.CreateFromAxisAngle(System.Numerics.Vector3.UnitY, rad);
        ViewMatrix = Matrix4x4.Transform(ViewMatrix, quat);
    }
    public void ScaleViewMatrix(float scale)
    {
        ViewMatrix = Matrix4x4.Multiply(ViewMatrix, scale);
    }

    private static void PrintMatrix(string name, Matrix4x4 matrix)
    {
        Console.WriteLine(name);

        for (int row = 0; row < 4; row++)
        {
            Console.Write("[ ");

            for (int column = 0; column < 4; column++)
            {
                Console.Write(matrix[row, column].ToString("N8") + ", ");
            }

            Console.WriteLine("]");
        }

        Console.Write("\n");
    }
    private static void PrintMatrix(string name, Matrix3x4 matrix)
    {
        Console.WriteLine(name);

        for (int row = 0; row < 3; row++)
        {
            Console.Write("[ ");

            for (int column = 0; column < 4; column++)
            {
                Console.Write(matrix[row, column].ToString("N8") + ", ");
            }

            Console.WriteLine("]");
        }
        
        Console.Write("\n");
    }
}