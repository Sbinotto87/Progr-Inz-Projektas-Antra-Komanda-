using Assets.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
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
    public const int Height = 256;
    public const int Width = 16;


    public int[,,] blocks; //3d array of blocks in the world (-1 denotes air block)

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
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // fixes array size so that we dont overflow via max size chunk
        var combineInstance = new List<CombineInstance>();
        for (int i = 0; i < Width; i++)
        {
            for (int j = offset; j < thickness + offset; j++)
            {
                for (int k = 0; k < Width; k++)
                {
                    if (BlockID != -1)
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
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // fixes array size so that we dont overflow via max size chunk
        var combineInstance = new List<CombineInstance>();

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                for (int k = 0; k < Width; k++)
                {
                    if (blocks[i, j, k] != -1 && !CheckIFBLockIsSurrounded(i, j, k))
                    {

                        Mesh mesh = Cube.GenerateMesh(new Vector3(i, j, k), blocks[i, j, k]);
                        CombineInstance temp = new CombineInstance();
                        temp.mesh = mesh;
                        temp.transform = meshFilter.transform.localToWorldMatrix;
                        combineInstance.Add(temp);
                    }
                }
            }
        }
        combinedMesh.CombineMeshes(combineInstance.ToArray());
        meshFilter.mesh = combinedMesh;
    }

    /// <summary>
    /// helper method to create the blocks array and fill it with -1 (air blocks)
    /// </summary>
    void InitializeBlocks()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                for (int z = 0; z < Width; z++)
                    blocks[x, y, z] = -1;
    }
    /// <summary>
    /// checks if the block at the specified coordinates is surrounded on all sides by blocks
    /// </summary>
    /// <param name="x">block x coord</param>
    /// <param name="y">block y coord</param>
    /// <param name="z">block z coord/param>
    /// <returns>true if surrounded, false if at least 1 side is exposed to air</returns>
    bool CheckIFBLockIsSurrounded(int x, int y, int z)
    {
        if(x !=0 && y !=0 && z !=0 &&//corner
           x != Width -1 && y!= Height -1 && z != Width - 1 //eliminates all edge blocks from check so so that no indexOutOfBounds
            )
        {
            //Debug.Log(String.Format("x: {0} y: {1} z: {2}", x.ToString(), y.ToString(), z.ToString()));
            if (blocks[x + 1, y, z] != -1 && blocks[x - 1, y, z] != -1 && //x dirrection
                blocks[x, y + 1, z] != -1 && blocks[x, y - 1, z] != -1 && //y dirrection
                blocks[x, y, z + 1] != -1 && blocks[x, y, z - 1] != -1  ) //z dirrection
                return true;
        }
        return false;
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
        chunkObject.transform.position = new Vector3(coord.x * Width / 2, 0f, coord.z * Width / 2);

        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        world = _world;

        chunkObject.transform.SetParent(world.transform);
        meshRenderer.material = world.material;

        chunkObject.name = coord.x + ", " + coord.z;

        blocks = new int[Width, Height, Width];

        InitializeBlocks();

        //world gen 
        //CreateLayerOfBlocks(0, 255, 0);//layer of 0 blocks
        CreateLayerOfBlocks(0, 1, 0);//layer of 0 blocks
        CreateLayerOfBlocks(2, 4, 1);//4 layer of 1 blocks
        CreateLayerOfBlocks(1, 3, 5);//3 layers of 2 blocks
        blocks[10, 8, 10] = 1;
        CreateChunkBlocks();
    }
}
