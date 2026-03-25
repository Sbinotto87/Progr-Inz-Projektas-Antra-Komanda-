using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    //comment so i can merge
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
        public const int WorldSize = 100;
        /// <summary>
        /// view distance from player
        /// </summary>
        public int viewDistance;
        /// <summary>
        /// world size in blocks, used for perlin noise bounds
        /// </summary>
        public const int WorldSizeInBlocks = WorldSize * Chunk.Width;
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
        private void Awake()
        {
            spawnPosition = spawnPosition = new Vector3((WorldSize * Chunk.Width) / 2f, Chunk.Height + 2f, (WorldSize * Chunk.Width) / 2f);
            var player = Instantiate(Player, spawnPosition, Quaternion.identity); //spawns player

            player.name = Player.name;
        }
        private void Start()
        {
            MyBlocks = GameObject.Find("Block").GetComponent<Blocks>();
            Seed = UnityEngine.Random.Range(0, int.MaxValue);
            UnityEngine.Random.InitState(Seed);
            DayTime = 0;
            CurrentDay = 0;
            Tick = 0;
            viewDistance = 2;

            GenerateWorld();
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            playerLastChunkCoord = GetChunkCoordFromVector3(playerTransform.position);
        }
        private void Update()
        {
            playerChunkCoord = GetChunkCoordFromVector3(playerTransform.position);

            if (!playerChunkCoord.Equals(playerLastChunkCoord)) //if player moved from chunk, update 
            {
                CheckViewDistance();
                playerLastChunkCoord = playerChunkCoord;
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
            for (int x = (WorldSize / 2) - viewDistance; x < (WorldSize / 2) + viewDistance; x++)
            {
                for (int z = (WorldSize / 2) - viewDistance; z < (WorldSize / 2) + viewDistance; z++)
                {
                    CreateChunk(new ChunkCoord(x, z));
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

            activeChunks.Add(new ChunkCoord(coord.x, coord.z));
        }
        /// <summary>
        /// gets voxel at position based on perlin
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>the voxel that should be at the pos based on perlin noisek</returns>
        public int GetVoxel(Vector3 pos)
        {

            int yPos = Mathf.FloorToInt(pos.y);

            /* IMMUTABLE PASS */

            // If outside world, return air.
            if (!IsVoxelInWorld(pos))
                return -1;
            // If bottom block of chunk, return block 1.
            if (yPos == 0)
                return 0;

            /* BASIC TERRAIN PASS */
            int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * PerlinNoise.Get2DPerlinNoise(new Vector2(pos.x, pos.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
            byte voxelValue = 0;

            if (yPos == terrainHeight) // grass
                voxelValue = 1;
            else if (yPos < terrainHeight && yPos > terrainHeight - 4) //dirt (add drit bruh why do we not have dirt
                voxelValue = 1;
            else if (yPos > terrainHeight)
                return -1; //air
            else
                voxelValue = 0; //stone

            /* SECOND PASS */

            //second pass is for random nodes of stuff in terrain like dirt in terrain in mc
            /*
            if (voxelValue == 2)
            {
                foreach (Lode lode in biome.lodes)
                {
                    if (yPos > lode.minHeight && yPos < lode.maxHeight)
                    {
                        if (PerlinNoise.Get3DPerlinNoise(pos, lode.noiseOffset, lode.scale, lode.threshold))
                            voxelValue = lode.blockID;
                    }
                }
            }
            */
            return voxelValue;
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
                            CreateChunk(new ChunkCoord(x, z));
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
