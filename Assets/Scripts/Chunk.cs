using Assets.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

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

    /// <summary>
    /// mesh information
    /// </summary>
    int triangleIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public int[,,] blocks; //3d array of blocks in the world (-1 denotes air block)
    /// <summary>
    /// block id
    /// </summary>
    public Blocks MyBlocks = null;
    /// <summary>
    /// chunk status for gameplay and render distance
    /// </summary>
    public bool isActive
    {
        get { return chunkObject.activeSelf; }
        set { chunkObject.SetActive(value); }
    }
    /// <summary>
    /// Creates blocks in the array based off the perlin noise generated in the other class
    /// </summary>
    public void PopulateBlockArray()
    {

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                for (int k = 0; k < Width; k++)
                {
                    blocks[i, j, k] = world.GetVoxel(new Vector3(i, j, k) + position);
                }
            }
        }
    }
    /// <summary>
    /// creates a mesh based on the data in the lists
    /// </summary>
    public void CreateChunkMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        MeshCollider col = chunkObject.GetComponent<MeshCollider>();

        if (col == null)
            col = chunkObject.AddComponent<MeshCollider>();

        col.sharedMesh = mesh;
    }
    /// <summary>
    /// adds mesh data to the lists in this class based on the data in the block array
    /// </summary>
    void CreateMeshData()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                for (int k = 0; k < Width; k++)
                {
                    if (blocks[i, j, k] != -1)
                    {
                        if (MyBlocks.block[blocks[i, j, k]].isSolid)
                            AddVoxelDataToChunk(new Vector3(i, j, k));
                    }
                }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    void AddVoxelDataToChunk(Vector3 pos)
    {
        for (int i = 0; i < 6; i++)
        {
            if (!CheckIfBlockIsSolid(pos + Voxel.faceChecks[i]))
            {
                int blockID = blocks[(int)pos.x, (int)pos.y, (int)pos.z];

                vertices.Add(pos + Voxel.Vertices[Voxel.Faces[i, 0]]);
                vertices.Add(pos + Voxel.Vertices[Voxel.Faces[i, 1]]);
                vertices.Add(pos + Voxel.Vertices[Voxel.Faces[i, 2]]);
                vertices.Add(pos + Voxel.Vertices[Voxel.Faces[i, 3]]);

                AddTexture(MyBlocks.block[blockID].faces[i]);

                triangles.Add(triangleIndex);
                triangles.Add(triangleIndex + 1);
                triangles.Add(triangleIndex + 2);
                triangles.Add(triangleIndex + 2);
                triangles.Add(triangleIndex + 3);
                triangles.Add(triangleIndex);
                triangleIndex += 4;

            }
        }
    }
    /// <summary>
    /// cheks if the block is solid(also does checks for blocks outside the chunk)
    /// </summary>
    /// <param name="pos">position in chunk</param>
    /// <returns>true if block is solid, false otherwise</returns>
    bool CheckIfBlockIsSolid(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x, y, z))
        {
            if(world.GetVoxel(pos + position) != -1)
                return MyBlocks.block[world.GetVoxel(pos + position)].isSolid;
        }
        if (y < 0) return true;
        if (x < 0 || x > Width - 1 || y < 0 || y > Height - 1 || z < 0 || z > Width - 1 || blocks[x, y, z] == -1)
            return false;
        return MyBlocks.block[blocks[x, y, z]].isSolid;

    }
    /// <summary>
    /// chesk if voxel within the limits of the chunk
    /// </summary>
    /// <param name="x">block x coord</param>
    /// <param name="y">block y coord</param>
    /// <param name="z">block z coord</param>
    /// <returns>true if block is in chunk, false otherwise</returns>
    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > Width - 1 || y < 0 || y > Height - 1 || z < 0 || z > Width - 1)
            return false;
        else
            return true;
    }
    /// <summary>
    /// Aplies texture info to face
    /// </summary>
    /// <param name="textureID">texture id to add from the atlas</param>
    void AddTexture(int textureID)
    {
        float y = textureID / Voxel.TextureAtlasWidth;
        float x = textureID - y * Voxel.TextureAtlasWidth;
        x *= Voxel.NormalizedBlockSize;
        y *= Voxel.NormalizedBlockSize;
        y = 1f - y - Voxel.NormalizedBlockSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + Voxel.NormalizedBlockSize));
        uvs.Add(new Vector2(x + Voxel.NormalizedBlockSize, y + Voxel.NormalizedBlockSize));
        uvs.Add(new Vector2(x + Voxel.NormalizedBlockSize, y));
    }

    public Vector3 position
    {
        get { return chunkObject.transform.position; }
    }

    /// <summary>
    /// creates chunk game object
    /// </summary>
    /// <param name="_coord">coordinates</param>
    /// <param name="_world">idk unity needs this</param>
    public Chunk(ChunkCoord Coord, World World, Blocks MyBlocks)
    {
        this.MyBlocks = MyBlocks;
        this.coord = Coord;
        this.world = World;

        chunkObject = new GameObject();
        chunkObject.transform.position = new Vector3(coord.x * Width, 0f, coord.z * Width);

        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshFilter = chunkObject.AddComponent<MeshFilter>();

        chunkObject.transform.SetParent(world.transform);
        meshRenderer.material = world.material;

        chunkObject.name = coord.x + ", " + coord.z;

        blocks = new int[Width, Height, Width];

        //InitializeBlocks();


        //world gen  testing
        //CreateLayerOfBlocks(0, 255, 0);//layer of 0 blocks
        //CreateLayerOfBlocks(0, 1, 0);//layer of 0 blocks
        //CreateLayerOfBlocks(2, 4, 1);//4 layer of 1 blocks
        //CreateLayerOfBlocks(1, 3, 5);//3 layers of 2 blocks
        //blocks[10, 8, 10] = 1;
        PopulateBlockArray();

        CreateMeshData();

        CreateChunkMesh();
    }

    //Legacy code from this point onwards
    //-------------------------------------------------------------------------------------------------
    //OBSOLETE
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
    //OBSOLETE
    /// <summary>
    /// Creates a flat layer of blocks
    /// </summary>
    /// <param name="BlockID">id of blocks to fill the layer with</param>
    /// <param name="thickness">layer thickness</param>
    /// <param name="offset">how far is the layer from y=0</param>
    /// <returns>CombineInstance containing meshes of the layer of blocks</returns>
    public List<CombineInstance> RenderLayerOfBlocks(int BlockID, int thickness, int offset)
    {
        //NOT USED ANYMORE
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
                        temp.transform = Matrix4x4.identity;
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
    public void CreateChunkBlocksOld()
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
                        temp.transform = Matrix4x4.identity;
                        combineInstance.Add(temp);
                    }
                }
            }
        }
        combinedMesh.CombineMeshes(combineInstance.ToArray());
        meshFilter.mesh = combinedMesh;
    }
    //OBSOLETE
    /// <summary>
    /// checks if the block at the specified coordinates is surrounded on all sides by blocks
    /// </summary>
    /// <param name="x">block x coord</param>
    /// <param name="y">block y coord</param>
    /// <param name="z">block z coord/param>
    /// <returns>true if surrounded, false if at least 1 side is exposed to air</returns>
    bool CheckIFBLockIsSurrounded(int x, int y, int z)
    {
        if (x != 0 && y != 0 && z != 0 &&//corner
           x != Width - 1 && y != Height - 1 && z != Width - 1 //eliminates all edge blocks from check so so that no indexOutOfBounds
            )
        {
            //Debug.Log(String.Format("x: {0} y: {1} z: {2}", x.ToString(), y.ToString(), z.ToString()));
            if (blocks[x + 1, y, z] != -1 && blocks[x - 1, y, z] != -1 && //x dirrection
                blocks[x, y + 1, z] != -1 && blocks[x, y - 1, z] != -1 && //y dirrection
                blocks[x, y, z + 1] != -1 && blocks[x, y, z - 1] != -1) //z dirrection
                return true;
        }
        return false;
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

    public void UpdateChunk()
    {
        // Reset mesh data
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        triangleIndex = 0;

        // Rebuild
        CreateMeshData();
        CreateChunkMesh();
    }

    public void UpdateNeighborChunks(int x, int z)
    {
        // LEFT
        if (x == 0 && coord.x > 0)
        {
            var n = world.chunks[coord.x - 1, coord.z];
            if (n != null) n.UpdateChunk();
        }

        // RIGHT
        if (x == Width - 1 && coord.x < World.WorldSize - 1)
        {
            var n = world.chunks[coord.x + 1, coord.z];
            if (n != null) n.UpdateChunk();
        }

        // BACK
        if (z == 0 && coord.z > 0)
        {
            var n = world.chunks[coord.x, coord.z - 1];
            if (n != null) n.UpdateChunk();
        }

        // FRONT
        if (z == Width - 1 && coord.z < World.WorldSize - 1)
        {
            var n = world.chunks[coord.x, coord.z + 1];
            if (n != null) n.UpdateChunk();
        }
    }
}
