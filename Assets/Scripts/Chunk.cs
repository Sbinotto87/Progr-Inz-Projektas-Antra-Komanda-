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

    public static int ChunkX;
    public static int ChunkY;

    public static readonly int Height = 256;
    public static readonly int Width = 16;

    int triangleIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public int[,,] blocks;
    public Blocks MyBlocks = null;

    public bool isActive
    {
        get { return chunkObject.activeSelf; }
        set { chunkObject.SetActive(value); }
    }

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

    void AddVoxelDataToChunk(Vector3 pos)
    {
        int blockID = blocks[(int)pos.x, (int)pos.y, (int)pos.z];

        if (blockID == -1)
            return;

        for (int i = 0; i < 6; i++)
        {
            bool neighborSolid = CheckIfBlockIsSolid(pos + Voxel.faceChecks[i]);

            if (!neighborSolid)
            {
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

    bool CheckIfBlockIsSolid(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x, y, z))
        {
            int id = world.GetVoxel(pos + position);

            if (id == 4)
                return false;

            if (id != -1)
                return MyBlocks.block[id].isSolid;
        }

        if (y < 0) return true;

        if (x < 0 || x > Width - 1 ||
            y < 0 || y > Height - 1 ||
            z < 0 || z > Width - 1 ||
            blocks[x, y, z] == -1)
            return false;

        int blockId = blocks[x, y, z];

        if (blockId == 4)
            return false;

        return MyBlocks.block[blockId].isSolid;
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > Width - 1 || y < 0 || y > Height - 1 || z < 0 || z > Width - 1)
            return false;
        return true;
    }

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

        PopulateBlockArray();
    }

    public void UpdateChunk()
    {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        triangleIndex = 0;

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
