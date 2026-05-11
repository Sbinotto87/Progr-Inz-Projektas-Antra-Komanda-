using Assets.Scripts;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.VisualScripting;

public class Chunk
{
    public MeshRenderer transparentMeshRenderer;
    public MeshFilter transparentMeshFilter;
    GameObject transparentObject;


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
    List<Vector3> normals = new List<Vector3>();

    /// <summary>
    /// transparent mesh information
    /// </summary>
    int transparentTriangleIndex = 0;
    List<Vector3> transparentVertices = new List<Vector3>();
    List<int> transparentTriangles = new List<int>();
    List<Vector2> transparentUvs = new List<Vector2>();
    List<Vector3> transparentNormals = new List<Vector3>();

    public int[,,] blocks; //3d array of blocks in the world (-1 denotes air block)
    /// <summary>
    /// block id
    /// </summary>
    public Blocks MyBlocks = null;
    /// <summary>
    /// chunk status for gameplay and render distance
    /// </summary>
    /// 
    public bool isRadioactive;

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
        isRadioactive = false;
        int totalBlocks = Width * Height * Width;
        NativeArray<int> jobResult = new NativeArray<int>(totalBlocks, Allocator.TempJob);
        NativeArray<bool> radioactivity = new NativeArray<bool>(1, Allocator.TempJob);
        biome = world.biome;

        var job = new ChunkDataJob
        {
            ResultBlocks = jobResult,
            isRadioactive = radioactivity,
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
        Parallel.For(0, totalBlocks, i =>
        {
            int x = i % Width;
            int y = (i / Width) % Height;
            int z = i / (Width * Height);

            blocks[x, y, z] = jobResult[i];
        });
        isRadioactive = radioactivity[0];
        if(radioactivity[0]) UnityEngine.Debug.Log("generatedRadioactivChunk");
        jobResult.Dispose();
        radioactivity.Dispose();
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

        Mesh transMesh = new Mesh();
        transMesh.vertices = transparentVertices.ToArray();
        transMesh.triangles = transparentTriangles.ToArray();
        transMesh.uv = transparentUvs.ToArray();
        transMesh.normals = transparentNormals.ToArray();
        transMesh.RecalculateBounds();
        transparentMeshFilter.mesh = transMesh;
    }
    /// <summary>
    /// adds mesh data to the lists in this class based on the data in the block array
    /// </summary>
    public void CreateMeshData()
    {
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
        int blockID = blocks[(int)pos.x, (int)pos.y, (int)pos.z];
        bool isBlockTransparent = MyBlocks.block[blockID].isTransparent;
        MeshType mesh = MyBlocks.block[blockID].mesh;

        for (int i = 0; i < 6; i++)
        {

            if (CheckIfBlockIsSolid(pos + Voxel.faceChecks[i], blockID))
            {
                List<Vector3> vList = isBlockTransparent ? transparentVertices : vertices;
                List<int> tList = isBlockTransparent ? transparentTriangles : triangles;
                List<Vector2> uList = isBlockTransparent ? transparentUvs : uvs;
                List<Vector3> nList = isBlockTransparent ? transparentNormals : normals;
                int tIndex = isBlockTransparent ? transparentTriangleIndex : triangleIndex;

    //            Full,
    //Grass,
    //Nf0875,//Not full and % of how much not full from top, ie Nf05 is half a block tall, like a slab
    //Nf075,
    //Nf0625,
    //Nf05,
    //Nf0375,
    //Nf025,
    //Nf0125

                switch (mesh)
                {
                    case MeshType.Full:
                        vList.Add(pos + Voxel.Vertices[Voxel.Faces[i, 0]]);
                        vList.Add(pos + Voxel.Vertices[Voxel.Faces[i, 1]]);
                        vList.Add(pos + Voxel.Vertices[Voxel.Faces[i, 2]]);
                        vList.Add(pos + Voxel.Vertices[Voxel.Faces[i, 3]]);
                        break;
                    case MeshType.Grass:
                        vList.Add(pos + Voxel.Vertices[Voxel.GrassFaces[i, 0]]);
                        vList.Add(pos + Voxel.Vertices[Voxel.GrassFaces[i, 1]]);
                        vList.Add(pos + Voxel.Vertices[Voxel.GrassFaces[i, 2]]);
                        vList.Add(pos + Voxel.Vertices[Voxel.GrassFaces[i, 3]]);
                        break;
                    case MeshType.Nf0875:
                        vList.Add(pos + Voxel.Vertices0875[Voxel.Faces[i, 0]]);
                        vList.Add(pos + Voxel.Vertices0875[Voxel.Faces[i, 1]]);
                        vList.Add(pos + Voxel.Vertices0875[Voxel.Faces[i, 2]]);
                        vList.Add(pos + Voxel.Vertices0875[Voxel.Faces[i, 3]]);
                        break;
                    case MeshType.Nf075:
                        vList.Add(pos + Voxel.Vertices075[Voxel.Faces[i, 0]]);
                        vList.Add(pos + Voxel.Vertices075[Voxel.Faces[i, 1]]);
                        vList.Add(pos + Voxel.Vertices075[Voxel.Faces[i, 2]]);
                        vList.Add(pos + Voxel.Vertices075[Voxel.Faces[i, 3]]);
                        break;
                    case MeshType.Nf0625:
                        vList.Add(pos + Voxel.Vertices0625[Voxel.Faces[i, 0]]);
                        vList.Add(pos + Voxel.Vertices0625[Voxel.Faces[i, 1]]);
                        vList.Add(pos + Voxel.Vertices0625[Voxel.Faces[i, 2]]);
                        vList.Add(pos + Voxel.Vertices0625[Voxel.Faces[i, 3]]);
                        break;
                    case MeshType.Nf05:
                        vList.Add(pos + Voxel.Vertices05[Voxel.Faces[i, 0]]);
                        vList.Add(pos + Voxel.Vertices05[Voxel.Faces[i, 1]]);
                        vList.Add(pos + Voxel.Vertices05[Voxel.Faces[i, 2]]);
                        vList.Add(pos + Voxel.Vertices05[Voxel.Faces[i, 3]]);
                        break;
                    case MeshType.Nf0375:
                        vList.Add(pos + Voxel.Vertices0375[Voxel.Faces[i, 0]]);
                        vList.Add(pos + Voxel.Vertices0375[Voxel.Faces[i, 1]]);
                        vList.Add(pos + Voxel.Vertices0375[Voxel.Faces[i, 2]]);
                        vList.Add(pos + Voxel.Vertices0375[Voxel.Faces[i, 3]]);
                        break;
                    case MeshType.Nf025:
                        vList.Add(pos + Voxel.Vertices0875[Voxel.Faces[i, 0]]);
                        vList.Add(pos + Voxel.Vertices0875[Voxel.Faces[i, 1]]);
                        vList.Add(pos + Voxel.Vertices0875[Voxel.Faces[i, 2]]);
                        vList.Add(pos + Voxel.Vertices0875[Voxel.Faces[i, 3]]);
                        break;
                    case MeshType.Nf0125:
                        vList.Add(pos + Voxel.Vertices0125[Voxel.Faces[i, 0]]);
                        vList.Add(pos + Voxel.Vertices0125[Voxel.Faces[i, 1]]);
                        vList.Add(pos + Voxel.Vertices0125[Voxel.Faces[i, 2]]);
                        vList.Add(pos + Voxel.Vertices0125[Voxel.Faces[i, 3]]);
                        break;
                    default:
                        vList.Add(pos + Voxel.Vertices[Voxel.Faces[i, 0]]);
                        vList.Add(pos + Voxel.Vertices[Voxel.Faces[i, 1]]);
                        vList.Add(pos + Voxel.Vertices[Voxel.Faces[i, 2]]);
                        vList.Add(pos + Voxel.Vertices[Voxel.Faces[i, 3]]);
                        break;
                }

                Vector3 normal = Voxel.faceChecks[i];
                for (int n = 0; n < 4; n++) nList.Add(normal);

                AddTexture(MyBlocks.block[blockID].faces[i], uList);

                tList.Add(tIndex);
                tList.Add(tIndex + 1);
                tList.Add(tIndex + 2);
                tList.Add(tIndex + 2);
                tList.Add(tIndex + 3);
                tList.Add(tIndex);

                if (isBlockTransparent) transparentTriangleIndex += 4;
                else triangleIndex += 4;
            }
        }
    }
    /// <summary>
    /// cheks if the block is solid(also does checks for blocks outside the chunk)
    /// </summary>
    /// <param name="pos">position in chunk</param>
    /// <returns>true if block is solid, false otherwise</returns>
    bool CheckIfBlockIsSolid(Vector3 pos, int currentBlockID)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        if (y < 0) return false;
        int neighborID;

        if (!IsVoxelInChunk(x, y, z))
        {
            neighborID = world.GetVoxel(pos + position);
        }
        else
        {
            neighborID = blocks[x, y, z];
        }

        if (neighborID == -1) return true;

        if (neighborID == currentBlockID && MyBlocks.block[currentBlockID].isTransparent)
        {
            return false;
        }
        return MyBlocks.block[neighborID].isTransparent || !MyBlocks.block[neighborID].isSolid;
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
    void AddTexture(int textureID, List<Vector2> targetUvList)
    {
        float y = textureID / Voxel.TextureAtlasWidth;
        float x = textureID - y * Voxel.TextureAtlasWidth;
        x *= Voxel.NormalizedBlockSize;
        y *= Voxel.NormalizedBlockSize;
        y = 1f - y - Voxel.NormalizedBlockSize;

        targetUvList.Add(new Vector2(x, y));
        targetUvList.Add(new Vector2(x, y + Voxel.NormalizedBlockSize));
        targetUvList.Add(new Vector2(x + Voxel.NormalizedBlockSize, y + Voxel.NormalizedBlockSize));
        targetUvList.Add(new Vector2(x + Voxel.NormalizedBlockSize, y));
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
        chunkObject.transform.SetParent(world.transform);
        chunkObject.name = coord.x + ", " + coord.z;

        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer.material = world.material;

        transparentObject = new GameObject();
        transparentObject.transform.SetParent(chunkObject.transform);
        transparentObject.transform.localPosition = Vector3.zero;
        transparentObject.name = "Transparent" + coord.x + ", " + coord.z;

        transparentMeshFilter = transparentObject.AddComponent<MeshFilter>();
        transparentMeshRenderer = transparentObject.AddComponent<MeshRenderer>();
        transparentMeshRenderer.material = world.transparentMaterial;

        blocks = new int[Width, Height, Width];

        PopulateBlockArray();
    }

    public void UpdateChunk()
    {
        // Reset all data
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        normals.Clear();
        triangleIndex = 0;

        transparentVertices.Clear();
        transparentTriangles.Clear();
        transparentUvs.Clear();
        transparentNormals.Clear();
        transparentTriangleIndex = 0;

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
        [NativeDisableParallelForRestriction]
        public NativeArray<bool> isRadioactive;

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
                if (y < 61)
                {
                    ResultBlocks[index] = 13; // oil
                    if (!isRadioactive[0])
                        isRadioactive[0] = true;
                }
                else
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
