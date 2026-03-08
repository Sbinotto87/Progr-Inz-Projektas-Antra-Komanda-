using UnityEngine;

/// <summary>
/// Voxel data
/// </summary>
public static class Voxel
{
    public static readonly int TextureAtlasWidth = 4;    //the width (in blocks) of texture atlas
    public static readonly float NormalizedBlockSize = 1.0f / TextureAtlasWidth; //normalized block size

    public static readonly Vector3[] Vertices =     //vertice coordinates of a block
    {
        new (1.0f, 0.0f, 0.0f),  //1
        new (1.0f, 1.0f, 0.0f),  //2
        new (1.0f, 1.0f, 1.0f),  //3
        new (1.0f, 0.0f, 1.0f),  //4
        new (0.0f, 0.0f, 0.0f),  //5
        new (0.0f, 1.0f, 0.0f),  //6
        new (0.0f, 1.0f, 1.0f),  //7
        new (0.0f, 0.0f, 1.0f)   //8
    };

    public static readonly int[,] Faces =           //faces of a block
    {
        //{0, 1, 2, 2, 3, 0}  the base vertices (for reference)
        
        {0, 1, 2, 3},     //front
        {7, 6, 5, 4},     //back
        {4, 5, 1, 0},     //right
        {3, 2, 6, 7},     //left
        {1, 5, 6, 2},     //top
        {3, 7, 4, 0}      //bottom
    };
}
