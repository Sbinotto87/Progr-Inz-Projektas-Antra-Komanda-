using Assets.Scripts;
using System.Collections.Generic;
using System.Drawing;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.LightTransport;
using static Unity.Collections.AllocatorManager;

public class Chunk : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    GameObject chunkObject;
    World world;
    public ChunkCoord coord;

    /// <summary>
    /// chunk coordinate in world
    /// </summary>
    public static int ChunkX;
    public static int ChunkY;

    /// <summary>
    /// chunk size 
    /// </summary>
    int Height = 8;
    int Width = 16;


    int[,,] blocks; //3d array of blocks in the world

    /// <summary>
    /// Creates a flat layer of blocks
    /// </summary>
    /// <param name="BlockID">id of blocks to fill the layer with</param>
    /// <param name="thickness">layer thickness</param>
    /// <param name="offset">how far is the layer from y=0</param>
    public void CreateLayerOfBlocks(int BlockID, int thickness, int offset)
    {

        for (int i = 0; i < Width; i++)
        {
            for (int j = offset; j < thickness + offset; j++)
            {
                for (int k = 0; k < Width; k++)
                {
                    blocks[i, j, k] = BlockID;
                }
            }
        }
    }
    /// <summary>
    /// Creates a flat layer of blocks
    /// </summary>
    /// <param name="BlockID">id of blocks to fill the layer with</param>
    /// <param name="thickness">layer thickness</param>
    /// <param name="offset">how far is the layer from y=0</param>
    /// <returns>CombineInstance containing meshes of the layer of blocks</returns>
    public List<CombineInstance> RenderLayerOfBlocks(int BlockID, int thickness, int offset)
    {
        Mesh combinedMesh = new Mesh();
        var combineInstance = new List<CombineInstance>();
        for (int i = 0; i < Width; i++)
        {
            for(int j = offset; j < thickness + offset; j++)
            {
                for (int k = 0; k < Width; k++)
                {
                    if(BlockID != -1)
                    {
                        Mesh mesh = Cube.GenerateMesh(new Vector3(i, j, k), BlockID);
                        CombineInstance temp = new CombineInstance();
                        temp.mesh = mesh;
                        temp.transform = meshFilter.transform.localToWorldMatrix;
                        combineInstance.Add(temp);
                    }
                }
            }
        }
        return combineInstance;
    }
    /// <summary>
    /// creates chunk based on blocks in the block data array
    /// </summary>
    public void CreateChunkBlocks()
    {
        Mesh combinedMesh = new Mesh();
        var combineInstance = new List<CombineInstance>();

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                for (int k = 0; k < Width; k++)
                {
                    Mesh mesh = Cube.GenerateMesh(new Vector3(i, j, k), blocks[i, j, k]);
                    CombineInstance temp = new CombineInstance();
                    temp.mesh = mesh;
                    temp.transform = meshFilter.transform.localToWorldMatrix;
                    combineInstance.Add(temp);
                }
            }
        }
        combinedMesh.CombineMeshes(combineInstance.ToArray());
        meshFilter.mesh = combinedMesh;
    }
    /// <summary>
    /// creates chunk game object
    /// </summary>
    /// <param name="_coord">coordinates</param>
    /// <param name="_world">idk unity needs this</param>
    public Chunk(ChunkCoord _coord, World _world)
    {

        coord = _coord;
        chunkObject = new GameObject();
        chunkObject.transform.position = new Vector3(coord.x * Width /2, 0f, coord.z * Width/2);

        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        world = _world;

        chunkObject.transform.SetParent(world.transform);
        meshRenderer.material = world.material;

        chunkObject.name = coord.x + ", " + coord.z;

        blocks = new int[Width, Height, Width];
        //world gen
        CreateLayerOfBlocks(0, 1, 0);//layer of 0 blocks
        CreateLayerOfBlocks(1, 4, 1);//4 layer of 1 blocks
        CreateLayerOfBlocks(2, 3, 5);//3 layers of 2 blocks

        CreateChunkBlocks();
    }
}
