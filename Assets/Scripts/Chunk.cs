using Assets.Scripts;
using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class Chunk
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    GameObject chunkObject;
    World world;
    public ChunkCoord coord;
    public BiomeData biome; //needed for multithreading


    /// <summary>
    /// chunk coordinate in world
    /// </summary>
    public static int ChunkX;
    public static int ChunkY;

    /// <summary>
    /// chunk size 
    /// </summary>
    public static readonly int Height = 256;
    public static readonly int Width = 16;

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
    /// 

    /// <summary>
    /// This is for normals, to avoid Recalculatemesh()
    /// </summary>
    List<Vector3> normals = new List<Vector3>();
    public bool isActive
    {
        get { return chunkObject.activeSelf; }
        set { chunkObject.SetActive(value); }
    }
    /// <summary>
    /// 
    /// </summary>
    public bool isPopulated;
    /// <summary>
    /// Creates blocks in the array based off the perlin noise generated in the other class
    /// </summary>
    public void PopulateBlockArray()
    {
        /*
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
        Debug.Log("generatedchunk");
        */
        int totalBlocks = Width * Height * Width;
        NativeArray<int> jobResult = new NativeArray<int>(totalBlocks, Allocator.TempJob);
        biome = world.biome;

        var job = new ChunkDataJob
        {
            ResultBlocks = jobResult,
            Width = Width,
            Height = Height,
            // Pass your ChunkCoord here
            ChunkCoord = new int2(this.coord.x, this.coord.z),
            offsetX = world.offsetX,
            offsetZ = world.offsetZ,
            TerrainHeight = biome.terrainHeight,
            TerrainScale = biome.terrainScale,
            SolidGroundHeight = biome.solidGroundHeight
        };

        // Schedule and Wait
        JobHandle handle = job.Schedule(totalBlocks, 64);
        handle.Complete();

        // Copy to your managed array
        for (int i = 0; i < totalBlocks; i++)
        {
            int x = i % Width;
            int y = (i / Width) % Height;
            int z = i / (Width * Height);
            blocks[x, y, z] = jobResult[i];
        }

        jobResult.Dispose();
        isPopulated = true;
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
        mesh.normals = normals.ToArray();

        mesh.RecalculateBounds();
        //mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        MeshCollider col = chunkObject.GetComponent<MeshCollider>();

        if (col == null)
            col = chunkObject.AddComponent<MeshCollider>();

        col.sharedMesh = mesh;
    }
    /// <summary>
    /// adds mesh data to the lists in this class based on the data in the block array
    /// </summary>
    public void CreateMeshData()
    {
        while (!isPopulated) { }
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                for (int k = 0; k < Width; k++)
                {
                    if (blocks[i, j, k] != -1)
                    {
                        AddVoxelDataToChunk(new Vector3(i, j, k));
                    }
                }
            }
        }
    }
    /// <summary>
    /// Adds voxel data in vertex, triangle and uv lists
    /// </summary>
    /// <param name="pos">Position of the block</param>
    void AddVoxelDataToChunk(Vector3 pos)
    {
       /* int blockID = blocks[(int)pos.x, (int)pos.y, (int)pos.z];
        for (int i = 0; i < 6; i++)
        {
            if (!CheckIfBlockIsSolid(pos + Voxel.faceChecks[i]))
            {
                if (blockID == 4 && i == 4) break;
                if (blockID == 4)
                {
                    vertices.Add(pos + Voxel.Vertices[Voxel.GrassFaces[i, 0]]);
                    vertices.Add(pos + Voxel.Vertices[Voxel.GrassFaces[i, 1]]);
                    vertices.Add(pos + Voxel.Vertices[Voxel.GrassFaces[i, 2]]);
                    vertices.Add(pos + Voxel.Vertices[Voxel.GrassFaces[i, 3]]);
                }
                else
                {
                    vertices.Add(pos + Voxel.Vertices[Voxel.Faces[i, 0]]);
                    vertices.Add(pos + Voxel.Vertices[Voxel.Faces[i, 1]]);
                    vertices.Add(pos + Voxel.Vertices[Voxel.Faces[i, 2]]);
                    vertices.Add(pos + Voxel.Vertices[Voxel.Faces[i, 3]]);
                }

                AddTexture(MyBlocks.block[blockID].faces[i]);

                triangles.Add(triangleIndex);
                triangles.Add(triangleIndex + 1);
                triangles.Add(triangleIndex + 2);
                triangles.Add(triangleIndex + 2);
                triangles.Add(triangleIndex + 3);
                triangles.Add(triangleIndex);
                triangleIndex += 4;
            }
        }*/
       int blockID = blocks[(int)pos.x, (int)pos.y, (int)pos.z];
       for (int i = 0; i < 6; i++)
       {
           if (!CheckIfBlockIsSolid(pos + Voxel.faceChecks[i]) || MyBlocks.block[blockID].isTransparent)
           {
               if (blockID == 4)
               {
                   vertices.Add(pos + Voxel.Vertices[Voxel.GrassFaces[i, 0]]);
                   vertices.Add(pos + Voxel.Vertices[Voxel.GrassFaces[i, 1]]);
                   vertices.Add(pos + Voxel.Vertices[Voxel.GrassFaces[i, 2]]);
                   vertices.Add(pos + Voxel.Vertices[Voxel.GrassFaces[i, 3]]);
               }
               else
               {
                   vertices.Add(pos + Voxel.Vertices[Voxel.Faces[i, 0]]);
                   vertices.Add(pos + Voxel.Vertices[Voxel.Faces[i, 1]]);
                   vertices.Add(pos + Voxel.Vertices[Voxel.Faces[i, 2]]);
                   vertices.Add(pos + Voxel.Vertices[Voxel.Faces[i, 3]]);
               }
                Vector3 normal = Voxel.faceChecks[i];

                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);

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
        if (x < 0 || x > Width - 1 || y < 0 || y > Height - 1 || z < 0 || z > Width - 1 || blocks[x, y, z] == -1 || MyBlocks.block[blocks[x, y, z]].isTransparent)
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

        //CreateMeshData();

        //CreateChunkMesh();
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
        normals.Clear();
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

    [BurstCompile]
    public struct ChunkDataJob : IJobParallelFor
    {
        public NativeArray<int> ResultBlocks;

        [ReadOnly] public int Width;
        [ReadOnly] public int Height;
        [ReadOnly] public int2 ChunkCoord; // Using int2 for (x, z)
        [ReadOnly] public float offsetX;
        [ReadOnly] public float offsetZ;

        [ReadOnly] public float TerrainHeight;
        [ReadOnly] public float TerrainScale;
        [ReadOnly] public int SolidGroundHeight;

        public void Execute(int index)
        {
            //flatten array su kočėlu
            int x = index % Width;
            int y = (index / Width) % Height;
            int z = index / (Width * Height);

            //convert coords to world coord for perlin noise
            float worldX = (ChunkCoord.x * Width) + x;
            float worldZ = (ChunkCoord.y * Width) + z;

            /* BASIC TERRAIN PASS */
            //perlinis garsas
            float2 noiseInput = new float2(worldX + 0.1f + offsetX, worldZ + 0.1f + offsetZ) * (TerrainScale / Width);
            float noiseValue = (noise.snoise(noiseInput) + 1f) * 0.5f; //normalize noise for 0 - 1
            int calculatedHeight = (int)(TerrainHeight * noiseValue) + SolidGroundHeight;

            //blocks
            if (y == 0)
                ResultBlocks[index] = 0; // Bedrock
            else if (y > calculatedHeight)
                ResultBlocks[index] = -1; // Air
            else if (y == calculatedHeight || (y < calculatedHeight && y > calculatedHeight - 4))
                ResultBlocks[index] = 1; // Dirt
            else
                ResultBlocks[index] = 0; // Stone

            /* SECOND PASS */
            //second pass is for random nodes of stuff in terrain like dirt in terrain in mc

            //if (voxelValue == 2)
            //    {
            //        foreach (Lode lode in biome.lodes)
            //        {
            //            if (yPos > lode.minHeight && yPos < lode.maxHeight)
            //        {
            //            if (PerlinNoise.Get3DPerlinNoise(pos, lode.noiseOffset, lode.scale, lode.threshold))
            //            voxelValue = lode.blockID;
            //        }
            //    }
            //}
        }
    }
}
