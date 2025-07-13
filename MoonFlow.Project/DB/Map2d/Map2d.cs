using System.Numerics;

using Nindot;

using Godot.Extension.Resources;
using System.Collections.Generic;
using System.Linq;
using Godot.Extension;
using System;
using Nindot.Byml;
using System.IO;

namespace MoonFlow.Project;

public partial class Map2d
{
    private readonly SarcFile Archive = null;
    internal string FileName = null;

    public readonly Godot.Image Texture = null;

    // A 4x4 matrix read straight from BYML data
    public Matrix4x4 ProjMatrix = Matrix4x4.Identity;

    // Internally this is stored as a 3x4 (3 row 4 column) matrix, however for matrix multiplication
    // it is easier to store the data in a 4x4 matrix. When written to the BYML the final row
    // is not written to disk
    public Matrix4x4 ViewMatrix = Matrix4x4.Identity;

    // This value is cached by the game's BYML, but is calculated by multiplying the ProjMatrix and ViewMatrix
    // It could be read from the BYML, but instead it is recalculated when the ProjMatrix or ViewMatrix
    // is modified.
    private Matrix4x4 ViewProjMatrix = Matrix4x4.Identity;

    public Map2d(string name, SarcFile sarc, BfresResource bfres)
    {
        Archive = sarc;
        FileName = name;

        // Fetch texture from bfres
        var textureName = name.TrimSuf(".byml");
        Texture = bfres.GetImage(textureName);

        // Load byml and handle matrix generation
        var byml = sarc.GetFileBYML(name);

        if (byml.TryGetValue(out List<object> proj, "ProjMatrix"))
            ProjMatrix = ImportMatrix4x4Data(proj.Cast<float>());

        if (byml.TryGetValue(out List<object> view, "ViewMatrix"))
            ViewMatrix = ImportMatrix3x4Data(view.Cast<float>());

        // The ViewProjMatrix can be calculated by multiplying the other two matricies instead of
        // reading from the BYML
        // if (byml.TryGetValue(out List<object> viewproj, "ViewProjMatrix"))
        //     ViewProjMatrix = ImportMatrix4x4Data(viewproj.Cast<float>());

        RecalculateViewProjMatrix();

        // PrintMatrix(nameof(ProjMatrix), ProjMatrix);
        // PrintMatrix(nameof(ViewMatrix), ViewMatrix);
        // PrintMatrix(nameof(ViewProjMatrix), ViewProjMatrix);
    }

    public void WriteMatrixDataToDb()
    {
        RecalculateViewProjMatrix();

        var output = new Dictionary<string, float[]>(3)
        {
            { "ProjMatrix", ConvertMatrixToList(ProjMatrix, 4) },
            { "ViewMatrix", ConvertMatrixToList(ViewMatrix, 3) },
            { "ViewProjMatrix", ConvertMatrixToList(ViewProjMatrix, 4) }
        };

        var stream = new MemoryStream();
        if (!BymlFileAccess.WriteFile(stream, output))
            throw new Exception("Failed to write BYML data!");

        Archive.Content[FileName] = stream.ToArray();
    }
    private static float[] ConvertMatrixToList(Matrix4x4 matrix, int rows)
    {
        var list = new float[rows * 4];

        for (int row = 0; row < rows; row++)
            for (int col = 0; col < 4; col++)
                list[(row * 4) + col] = matrix[row, col];

        return list;
    }

    #region Utilities

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

    public void RecalculateViewProjMatrix()
    {
        // Reset row 4 of the ViewMatrix to ensure it stays aligned with identity
        for (int column = 0; column < 4; column++)
            ViewMatrix[3, column] = Matrix4x4.Identity[3, column];

        ViewProjMatrix = Matrix4x4.Multiply(ProjMatrix, ViewMatrix);
    }

    public Map2d DuplicateMap()
    {
        return MemberwiseClone() as Map2d;
    }

    #endregion
}