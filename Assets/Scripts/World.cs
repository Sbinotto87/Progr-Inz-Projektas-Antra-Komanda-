using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Assets.Scripts
{
    
    public class World: MonoBehaviour
    {
        /// <summary>
        /// Player gameObject prefab
        /// </summary>
        [SerializeField]
        private GameObject Player;
        /// <summary>
        /// world size
        /// </summary>
        public static readonly int WorldSize = 100;
        /// <summary>
        /// view distance from player
        /// </summary>
        public int viewDistance;
        /// <summary>
        /// world size in blocks, used for perlin noise bounds
        /// </summary>
        public readonly int WorldSizeInBlocks = WorldSize * Chunk.Width;
        /// <summary>
        /// chunk array
        /// </summary>
        public Chunk[,] chunks = new Chunk[WorldSize, WorldSize];
        /// <summary>
        /// List of chunks in the players vision
        /// </summary>
        List<ChunkCoord> activeChunks = new List<ChunkCoord>();
        /// <summary>
        /// no clu, pavogiau koda
        /// </summary>
        public Material material;
        public Material transparentMaterial;
        /// <summary>
        /// world time
        /// </summary>
        public int DayTime; //1440 seconds (24 minutes, 1 irl second = 1 ingame minute
        public int CurrentDay; //event every 7 days?
        public int Tick;

        Blocks MyBlocks = null;

        /// <summary>
        /// signed 32bit int for seed
        /// </summary>
        public int Seed;
        public float offsetX;
        public float offsetZ;
        /// <summary>
        /// current biome to generate 
        /// </summary>
        public BiomeData biome;
        /// <summary>
        /// the spawn position(gets overwritten)
        /// </summary>
        Vector3 spawnPosition = new Vector3 (0, 100, 0);
        /// <summary>
        /// the chunk that the player is in
        /// </summary>
        ChunkCoord playerChunkCoord;
        /// <summary>
        /// the chunk that the player was previously in
        /// </summary>
        ChunkCoord playerLastChunkCoord;
        /// <summary>
        /// player position cause for some reason i cant pull it from gameObject player
        /// </summary>
        private Transform playerTransform;
        Queue<ChunkCoord> chunksToRender;
        public bool IsInRadiation;

        private void Awake()
        {
            spawnPosition = spawnPosition = new Vector3((WorldSize * Chunk.Width) / 2f, Chunk.Height - 5, (WorldSize * Chunk.Width) / 2f);
            var player = Instantiate(Player, spawnPosition, Quaternion.identity); //spawns player

            player.name = Player.name;
        }
        private void Start()
        {
            IsInRadiation = false;
            MyBlocks = GameObject.Find("Block").GetComponent<Blocks>();
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
            Seed = UnityEngine.Random.Range(0, 10000);
            UnityEngine.Random.InitState(Seed);
            UnityEngine.Debug.Log(Seed.ToString());
            offsetX = (float)(Seed % 10000);
            offsetZ = (float)((Seed / 100) % 10000);
            DayTime = 500;
            CurrentDay = 0;
            Tick = 0;
            viewDistance = 8;
            chunksToRender = new Queue<ChunkCoord> ();

            UnityEngine.Debug.Log("generating");
            GenerateWorld();
            UnityEngine.Debug.Log("generated");
            
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            playerLastChunkCoord = GetChunkCoordFromVector3(playerTransform.position);
        }
        private void Update()
        {
            playerChunkCoord = GetChunkCoordFromVector3(playerTransform.position);

            Stopwatch sw = Stopwatch.StartNew();
            if (!playerChunkCoord.Equals(playerLastChunkCoord)) //if player moved from chunk, update 
            {
                bool radiation = chunks[playerChunkCoord.x, playerChunkCoord.z].isRadioactive;
                if (radiation && !IsInRadiation)
                {
                    UnityEngine.Debug.Log("rads");
                    IsInRadiation = true;
                }
                else if(!radiation && IsInRadiation)
                {
                    IsInRadiation = false;
                }

                CheckViewDistance();
                playerLastChunkCoord = playerChunkCoord;
                UnityEngine.Debug.Log(sw.Elapsed);
            }
            if(chunksToRender.Count>0)
            {
                ChunkCoord coord = chunksToRender.Dequeue();
                CreateChunk(new ChunkCoord(coord.x, coord.z));
                chunks[coord.x, coord.z].CreateMeshData();
                chunks[coord.x, coord.z].CreateChunkMesh();
            }
        }
        /// <summary>
        /// tickrate, 20 ticks per second, used for ingame time
        /// </summary>
        private void FixedUpdate()
        {
            TickDayTime();
        }
        /// <summary>
        /// updates the ingame time (in seconds) every 20 tics
        /// </summary>
        private void TickDayTime()
        {
            switch (Tick)
            {
                case 19:
                    Tick = 0;
                    DayTime++;
                    if (DayTime == 1440 - 1) UpdateDayCounter();
                    break;
                default:
                    Tick++;
                    break;
            }
        }
        /// <summary>
        /// increments day counter and resets day time, called at midnight
        /// </summary>
        private void UpdateDayCounter()
        {
            DayTime = 0;
            CurrentDay++;
        }
        /// <summary>
        /// generates chunk coordinates for world and the chunks themselves
        /// </summary>
        private void GenerateWorld()
        {
            //Add block numbers to each chunk's block array
            for (int x = (WorldSize / 2) - viewDistance; x < (WorldSize / 2) + viewDistance; x++)
            {
                for (int z = (WorldSize / 2) - viewDistance; z < (WorldSize / 2) + viewDistance; z++)
                {
                    //CreateChunk(new ChunkCoord(x, z));
                    chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, MyBlocks);
                    
                }
            }
            
            Structures.GenerateMall(this, spawnPosition);
            Structures.GenerateBuildings(this, spawnPosition, (viewDistance - 2) * Chunk.Width, (viewDistance - 2) * Chunk.Width);
            
            for (int x = (WorldSize / 2) - viewDistance; x < (WorldSize / 2) + viewDistance; x++)
            {
                for (int z = (WorldSize / 2) - viewDistance; z < (WorldSize / 2) + viewDistance; z++)
                {
                    Structures.GenerateGrass(chunks[x, z]);
                    Structures.GenerateTrees(chunks[x, z]);
                    Structures.GenerateOres(chunks[x, z], 10);
                    
                    chunks[x, z].CreateMeshData();
                    chunks[x, z].CreateChunkMesh();
        
                    activeChunks.Add(new ChunkCoord(x, z));
                }
            }
            
            Player.transform.position = spawnPosition;
        }
        /// <summary>
        /// creates chunk game object
        /// </summary>
        /// <param name="coord">coordiate to create chunk at</param>
        private void CreateChunk(ChunkCoord coord)
        {
            chunks[coord.x, coord.z] = new Chunk(new ChunkCoord(coord.x, coord.z), this, MyBlocks);
            
            //Generate structures
            Structures.GenerateGrass(chunks[coord.x, coord.z]);
            Structures.GenerateTrees(chunks[coord.x, coord.z]);
            Structures.GenerateOres(chunks[coord.x, coord.z], 10);

            chunks[coord.x, coord.z].CreateMeshData();
            chunks[coord.x, coord.z].CreateChunkMesh();

            activeChunks.Add(new ChunkCoord(coord.x, coord.z));
        }
        ///// <summary>
        ///// gets voxel at position based on perlin
        ///// </summary>
        ///// <param name="pos"></param>
        ///// <returns>the voxel that should be at the pos based on perlin noisek</returns>
        //public int GetVoxel(Vector3 pos)
        //{

        //    int yPos = Mathf.FloorToInt(pos.y);

        //    /* IMMUTABLE PASS */

        //    // If outside world, return air.
        //    if (!IsVoxelInWorld(pos))
        //        return -1;
        //    // If bottom block of chunk, return block 1.
        //    if (yPos == 0)
        //        return 0;

        //    /* BASIC TERRAIN PASS */
        //    int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * PerlinNoise.Get2DPerlinNoise(new Vector2(pos.x, pos.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
        //    byte voxelValue = 0;

        //    if (yPos == terrainHeight) // grass
        //        voxelValue = 1;
        //    else if (yPos < terrainHeight && yPos > terrainHeight - 4) //dirt (add drit bruh why do we not have dirt
        //        voxelValue = 1;
        //    else if (yPos > terrainHeight)
        //        return -1; //air
        //    else
        //        voxelValue = 0; //stone

        //    /* SECOND PASS */

        //    //second pass is for random nodes of stuff in terrain like dirt in terrain in mc
        //    /*
        //    if (voxelValue == 2)
        //    {
        //        foreach (Lode lode in biome.lodes)
        //        {
        //            if (yPos > lode.minHeight && yPos < lode.maxHeight)
        //            {
        //                if (PerlinNoise.Get3DPerlinNoise(pos, lode.noiseOffset, lode.scale, lode.threshold))
        //                    voxelValue = lode.blockID;
        //            }
        //        }
        //    }
        //    */
        //    return voxelValue;
        //}
        public int GetVoxel(Vector3 pos)
        {
            if (!IsVoxelInWorld(pos))
                return -1;

            int x = Mathf.FloorToInt(pos.x);
            int y = Mathf.FloorToInt(pos.y);
            int z = Mathf.FloorToInt(pos.z);

            int chunkX = x / Chunk.Width;
            int chunkZ = z / Chunk.Width;

            if (chunkX >= 0 && chunkZ >= 0 &&
                chunkX < WorldSize && chunkZ < WorldSize)
            {
                Chunk chunk = chunks[chunkX, chunkZ];

                if (chunk != null)
                {
                    int localX = x - chunkX * Chunk.Width;
                    int localZ = z - chunkZ * Chunk.Width;

                    if (y >= 0 && y < Chunk.Height)
                        return chunk.blocks[localX, y, localZ];
                }
            }

            // fallback: terrain generation (ONLY for ungenerated areas)           
            return GenerateTerrainVoxel(x, y, z);
        }
        private int GenerateTerrainVoxel(int x, int y, int z)
        {
            if (y == 0)
                return 0;


            int terrainHeight =
                Mathf.FloorToInt(biome.terrainHeight *
                PerlinNoise.Get2DPerlinNoise(new Vector2(x, z), 0, biome.terrainScale, this.offsetX, this.offsetZ))
                + biome.solidGroundHeight;

            if (y > terrainHeight)
                if (y < 61)
                    return 11;
                else 
                return -1; // Air
            else if (y == terrainHeight || (y < terrainHeight && y > terrainHeight - 4))
                return 1; // dirt
            else
                return 0; // Stone
        }
        /// <summary>
        /// gets chunk coord from world coord
        /// </summary>
        /// <param name="pos">position in world</param>
        /// <returns>chunk coord that the world coord is in</returns>
        ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
        {
            int x = Mathf.FloorToInt(pos.x / Chunk.Width);
            int z = Mathf.FloorToInt(pos.z / Chunk.Width);

            return new ChunkCoord(x, z);
        }
        /// <summary>
        /// recalculates whuch chunks should be active
        /// </summary>
        void CheckViewDistance()
        {
            ChunkCoord coord = GetChunkCoordFromVector3(playerTransform.position);
            List<ChunkCoord> PreviouslyActiveChunks = new List<ChunkCoord>(activeChunks);

            //loop through chunks in view distance
            for (int x = coord.x - viewDistance; x < coord.x + viewDistance; x++)
            {
                for (int z = coord.z - viewDistance; z < coord.z + viewDistance; z++)
                {
                    //if not active
                    if(ChunkIsInWorld(new ChunkCoord(x, z)))
                    {
                        //create if doesnt exist
                        if (chunks[x,z] == null)
                        {
                            chunksToRender.Enqueue(new ChunkCoord(x, z));
                        }
                        //activate
                        else if (!chunks[x, z].isActive)
                        {
                            chunks[x, z].isActive = true;
                            activeChunks.Add(new ChunkCoord(x, z));
                        }
                    }
                    //remove chunk from previously active list
                    for (int i = 0; i < PreviouslyActiveChunks.Count; i++)
                    {

                        if (PreviouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                            PreviouslyActiveChunks.RemoveAt(i);

                    }
                }
            }
            //deactivate chunks that were previously active
            foreach (ChunkCoord c in PreviouslyActiveChunks)
                chunks[c.x, c.z].isActive = false;

        }
        /// <summary>
        /// used for checking view distance
        /// </summary>
        /// <param name="coord">chunk coordinate</param>
        /// <returns> true if chunk is within limits of world size</returns>
        bool ChunkIsInWorld(ChunkCoord coord)
        {
            if (coord.x > 0 && coord.x < WorldSize - 1 && coord.z > 0 && coord.z < WorldSize - 1)
                return true;
            else
                return false;
        }
        /// <summary>
        /// used for limiting block creation (based on perlin noise)
        /// </summary>
        /// <param name="pos">block location</param>
        /// <returns>true if within world limits</returns>
        bool IsVoxelInWorld(Vector3 pos)
        {
            if (pos.x >= 0 && pos.x < WorldSizeInBlocks && pos.y >= 0 && pos.y < Chunk.Height && pos.z >= 0 && pos.z < WorldSizeInBlocks)
                return true;
            else
                return false;
        }
    }
    /// <summary>
    /// chunk coordinate class
    /// </summary>
    public class ChunkCoord
    {
        public int x;
        public int z;
        public ChunkCoord(int x, int z)
        {
            this.x = x;
            this.z = z;
        }
        public bool Equals(ChunkCoord other)
        {
            if (this.x == other.x && this.z == other.z)
                return true;
            return false;
        }
    }
}
