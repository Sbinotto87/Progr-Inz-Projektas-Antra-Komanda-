using UnityEngine;

/// <summary>
/// Cube class with methods for generating blocks
/// </summary>
// OBSOLETE
public static class Cube
{
    /// <summary>
    /// Generates a mesh in the given position of the given block (using blockID)
    /// </summary>
    /// <param name="position">position of the block in 3D space</param>
    /// <param name="blockID">the ID of a block</param>
    /// <returns>generated Mesh object</returns>
    public static Mesh GenerateMesh(Vector3 position, int blockID)
    {
        Blocks myBlocks = GameObject.Find("Block").GetComponent<Blocks>();
        
        //Adding voxel data to arrays
        Vector3[] vertices = new Vector3[24];
        int [] triangles = new int[36];
        Vector2[] uv = new Vector2[24];
        int vertexIndex = 0;
        int faceIndex = 0;
        int triangleIndex = 0;
        int uvIndex = 0;
        for (int i = 0; i < 6; i++)
        {
            if (blockID == 4)
            {
                vertices[vertexIndex++] = Voxel.Vertices[Voxel.GrassFaces[i, 0]] + position;
                vertices[vertexIndex++] = Voxel.Vertices[Voxel.GrassFaces[i, 1]] + position;
                vertices[vertexIndex++] = Voxel.Vertices[Voxel.GrassFaces[i, 2]] + position;
                vertices[vertexIndex++] = Voxel.Vertices[Voxel.GrassFaces[i, 3]] + position;
            }
            else
            {
               vertices[vertexIndex++] = Voxel.Vertices[Voxel.Faces[i, 0]] + position;
               vertices[vertexIndex++] = Voxel.Vertices[Voxel.Faces[i, 1]] + position;
               vertices[vertexIndex++] = Voxel.Vertices[Voxel.Faces[i, 2]] + position;
               vertices[vertexIndex++] = Voxel.Vertices[Voxel.Faces[i, 3]] + position; 
            }

            triangles[faceIndex++] = triangleIndex;
            triangles[faceIndex++] = triangleIndex + 1;
            triangles[faceIndex++] = triangleIndex + 2;
            triangles[faceIndex++] = triangleIndex + 2;
            triangles[faceIndex++] = triangleIndex + 3;
            triangles[faceIndex++] = triangleIndex;
            triangleIndex += 4;
            AddTexture(myBlocks.block[blockID].faces[i], ref uv, ref uvIndex);
        }
        
        //Mesh creation
        Mesh mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles,
            uv = uv
        };
        mesh.RecalculateNormals();
        return mesh;
    }
    /// <summary>
    /// Applies texture to the current face
    /// </summary>
    /// <param name="textureID">the ID of a texture</param>
    /// <param name="uv">UV array of a mesh</param>
    /// <param name="index">index of the current element in the uv array</param>
    private static void AddTexture(int textureID, ref Vector2[] uv, ref int index)
    {
        float y = textureID / Voxel.TextureAtlasWidth;
        float x = textureID - y * Voxel.TextureAtlasWidth;
        x *= Voxel.NormalizedBlockSize;
        y *= Voxel.NormalizedBlockSize;
        y = 1f - y - Voxel.NormalizedBlockSize;
        
        uv[index++] = new Vector2(x, y);
        uv[index++] = new Vector2(x, y + Voxel.NormalizedBlockSize);
        uv[index++] = new Vector2(x + Voxel.NormalizedBlockSize, y + Voxel.NormalizedBlockSize);
        uv[index++] = new Vector2(x + Voxel.NormalizedBlockSize, y);
    }
}
